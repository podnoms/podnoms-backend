using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistsJob : IHostedJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly HelpersSettings _helpersSettings;
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ProcessPlaylistsJob> _logger;
        private readonly IYouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AudioDownloader _downloader;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;

        public ProcessPlaylistsJob(
            IPlaylistRepository playlistRepository,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IYouTubeParser ytParser,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<AppSettings> appSettings,
            AudioDownloader downloader,
            ILoggerFactory logger,
            IYouTubeParser youTubeParser,
            MixcloudParser mixcloudParser) {
            _unitOfWork = unitOfWork;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _youTubeParser = youTubeParser;
            _mixcloudParser = mixcloudParser;
            _playlistRepository = playlistRepository;
            _entryRepository = entryRepository;
            _helpersSettings = helpersSettings.Value;
            _storageSettings = storageSettings.Value;
            _appSettings = appSettings.Value;
            _downloader = downloader;
            _logger = logger.CreateLogger<ProcessPlaylistsJob>();
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            _logger.LogDebug("Starting playlist processing");
            context.WriteLine("Starting playlist processing");
            var playlists = _playlistRepository.GetAll();
            // .Where(p => p.Id == Guid.Parse("544e9984-7ed5-4c76-10e6-08d70ff62e10"));
            foreach (var playlist in playlists) {
                await Execute(playlist.Id, context);
            }
            return true;
        }
        public async Task<bool> Execute(Guid playlistId, PerformContext context) {
            _logger.LogDebug($"Starting playlist processing for {playlistId}");
            context.WriteLine($"Starting playlist processing for {playlistId}");
            try {
                var playlist = await _playlistRepository.GetAsync(playlistId);
                var cutoffDate = await _playlistRepository.GetCutoffDate(playlistId);
                if (playlist is null)
                    return false;

                //first check quotas
                _logger.LogDebug("Checking quotas");
                var quota = playlist.Podcast.AppUser.DiskQuota ?? _storageSettings.DefaultUserQuota;
                var totalUsed = (await _entryRepository.GetAllForUserAsync(playlist.Podcast.AppUser.Id))
                    .Select(x => x.AudioFileSize)
                    .Sum();

                if (totalUsed >= quota) {
                    _logger.LogError($"Storage quota exceeded for {playlist.Podcast.AppUser.GetBestGuessName()}");
                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.NotifyUser(
                            playlist.Podcast.AppUser.Id.ToString(),
                            $"Failure processing playlist\n{playlist.Podcast.Title}\n",
                            $"Your have exceeded your storage quota of {quota.Bytes().ToString()}",
                            playlist.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                            playlist.Podcast.GetThumbnailUrl(_storageSettings.CdnUrl, _imageFileStorageSettings.ContainerName),
                            NotificationOptions.StorageExceeded
                        ));
                    return false;
                }
                _logger.LogDebug("Quotas passed");

                //check for active subscription
                var resultList = new List<ParsedItemResult>();
                var count = _storageSettings.DefaultEntryCount;
                if (_youTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    _logger.LogDebug("Parsing YouTube");
                    resultList = await _youTubeParser
                        .GetPlaylistItems(playlist.SourceUrl, cutoffDate, count);
                } else if (MixcloudParser.ValidateUrl(playlist.SourceUrl)) {
                    _logger.LogDebug("Parsing MixCloud");
                    var entries = await _mixcloudParser
                            .GetEntries(playlist.SourceUrl, count);
                    resultList = entries
                        .OrderBy(r => r.UploadDate)
                        .Take(_storageSettings.DefaultEntryCount)
                        .ToList();
                }
                _logger.LogDebug($"Found {resultList.Count} candidates");

                //order in reverse so the newest item is added first
                foreach (var item in resultList) {
                    if (playlist.PodcastEntries.Any(e => e.SourceItemId == item.Id))
                        continue;
                    await _trimPlaylist(playlist);
                    _logger.LogDebug($"Found candidate\n\tParsedId:{item.Id}\n\tPodcastId:{playlist.Podcast.Id}\n\t{playlist.Id}");
                    BackgroundJob
                        .Enqueue<ProcessPlaylistItemJob>(
                            service => service.Execute(item, playlist.Id, null)
                    );
                }
                _logger.LogDebug($"Finished playlists");
                return true;
            } catch (PlaylistExpiredException) {
                //TODO: Remove playlist and notify user
                _logger.LogInformation($"Playlist: {playlistId} cannot be found");
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                context.WriteLine($"ERROR(ProcessPlayListJob): {ex.Message}");
            }
            return false;
        }

        private async Task _trimPlaylist(Playlist playlist) {
            var currentCount = playlist.PodcastEntries.Count(r => playlist.PodcastEntries.Contains(r));
            if (currentCount >= _storageSettings.DefaultEntryCount) {
                _logger.LogError($"Entry count exceeded for {playlist.Podcast.AppUser.GetBestGuessName()}");
                var toDelete = playlist.PodcastEntries
                    .OrderByDescending(o => o.SourceCreateDate)
                    .Take(currentCount - _storageSettings.DefaultEntryCount + 1);

                foreach (var item in toDelete) {
                    await _entryRepository.DeleteAsync(item.Id);
                }
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
