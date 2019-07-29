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
            await File.AppendAllTextAsync
                (@"c:\temp\joblog.csv",
                $"\"ItemId\",\"PodcastId\",PlaylistId\",\"SourceUrl\",\"PodcastUrl\"{Environment.NewLine}"
            );
            var playlists = _playlistRepository.GetAll()
                .Where(p => p.PodcastId == Guid.Parse("d2d9e1b5-817e-41dd-ebb1-08d622cc9a76"));
            foreach (var playlist in playlists) {
                await Execute(playlist.Id, context);
            }
            return true;
        }
        [Mutex("ProcessPlaylistId_Job")]
        public async Task<bool> Execute(Guid playlistId, PerformContext context) {
            _logger.LogDebug($"Starting playlist processing for {playlistId}");
            context.WriteLine($"Starting playlist processing for {playlistId}");
            try {
                var playlist = await _playlistRepository.GetAsync(playlistId);
                if (playlist is null)
                    return false;

                //first check quotas
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

                //check for active subscription
                var resultList = new List<ParsedItemResult>();
                var count = _storageSettings.DefaultEntryCount;// - playlist.ParsedPlaylistItems.Count;
                if (_youTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    resultList = await _youTubeParser.GetPlaylistItems(playlist.SourceUrl, count);
                } else if (MixcloudParser.ValidateUrl(playlist.SourceUrl)) {
                    var entries = await _mixcloudParser
                            .GetEntries(playlist.SourceUrl, count);
                    resultList = entries
                        .OrderBy(r => r.UploadDate)
                        .ToList();
                }

                if (resultList is null) return true;
                //order in reverse so the newest item is added first
                foreach (var item in resultList) {

                    _logger.LogDebug($"Processing playlist item: {item.Id}");
                    if (playlist.ParsedPlaylistItems.Any(p => p.VideoId == item.Id)) continue;
                    await _trimPlaylist(playlist);

                    _logger.LogDebug($"Found missing item: {item.Id}");

                    playlist.ParsedPlaylistItems.Add(new ParsedPlaylistItem {
                        VideoId = item.Id,
                        VideoType = item.VideoType
                    });
                    await _unitOfWork.CompleteAsync();
                    _logger.LogDebug($"Found candidate\n\tParsedId:{item.Id}\n\tPodcastId:{playlist.Podcast.Id}\n\t{playlist.Id}");
                    await File.AppendAllTextAsync
                        (@"c:\temp\joblog.csv",
                        $"\"{item.Id}\",\"{playlist.Podcast.Id}\",\"{playlist.Id}\",\"{playlist.SourceUrl}\",\"{playlist.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl)}\"{Environment.NewLine}"
                    );
                    BackgroundJob
                        .Enqueue<ProcessPlaylistItemJob>(
                            service => service.Execute(item.Id, playlist.Id, null)
                    );
                }
                return true;
            } catch (PlaylistExpiredException e) {
                //TODO: Remove playlist and notify user
                _logger.LogInformation($"Playlist: {playlistId} cannot be found");
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                context.WriteLine($"ERROR(ProcessPlayListJob): {ex.Message}");
            }
            return false;
        }

        private async Task _trimPlaylist(Playlist playlist) {
            if (playlist.ParsedPlaylistItems.Count > _storageSettings.DefaultEntryCount) {
                _logger.LogError($"Entry count exceeded for {playlist.Podcast.AppUser.GetBestGuessName()}");
                var toDelete = playlist.ParsedPlaylistItems
                    .OrderByDescending(o => o.CreateDate)
                    .Skip(_storageSettings.DefaultEntryCount + 1);

                _playlistRepository.DeletePlaylistItems(toDelete.ToList());
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
