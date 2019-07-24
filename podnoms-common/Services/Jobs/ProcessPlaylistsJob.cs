using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using PodNoms.Common.Services.NYT.Models;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistsJob : IHostedJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly HelpersSettings _helpersSettings;
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ProcessPlaylistsJob> _logger;
        private readonly YouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;

        public ProcessPlaylistsJob(
            IPlaylistRepository playlistRepository,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<AppSettings> appSettings,
            ILoggerFactory logger,

            YouTubeParser youTubeParser,
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
            _logger = logger.CreateLogger<ProcessPlaylistsJob>();
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            _logger.LogDebug("Starting playlist processing");
            context.WriteLine("Starting playlist processing");

            var playlists = _playlistRepository.GetAll();
            // .Where(r => r.Id == Guid.Parse("0c6a947d-505a-4992-db12-08d6a4be70f7"));
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
                var downloader = new AudioDownloader(playlist.SourceUrl, _helpersSettings.Downloader);
                var info = downloader.GetInfo();
                var id = ((PlaylistDownloadInfo)downloader.RawProperties)?.Id;

                if (string.IsNullOrEmpty(id)) return true;

                var count = _storageSettings.DefaultEntryCount;// - playlist.ParsedPlaylistItems.Count;
                if (YouTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    var searchTerm = (playlist.SourceUrl.Contains("/user/")) ? "forUsername" : "id";
                    var entries = await _youTubeParser.GetPlaylistEntriesForId(id, count);
                    resultList = entries
                        .OrderBy(r => r.UploadDate)
                        .ToList();
                    //make sure the items are sorted in ascending date order
                    //so they will be processed in the order they were created
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
                    BackgroundJob.Enqueue<ProcessPlaylistItemJob>(service => service.Execute(item.Id, playlist.Id, null));
                }
                return true;
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
