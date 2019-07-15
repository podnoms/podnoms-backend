using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Processor;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistItemJob : IJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly AppSettings _appSettings;
        private readonly IPodcastRepository _podcastRepository;
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _imageStorageSettings;
        private readonly HelpersSettings _helpersSettings;
        private readonly ILogger<ProcessPlaylistItemJob> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPlaylistItemJob(
            IPlaylistRepository playlistRepository,
            IAudioUploadProcessService uploadService,
            IOptions<AppSettings> appSettings,
            IPodcastRepository podcastRepository,
            IOptions<ImageFileStorageSettings> imageStorageSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<HelpersSettings> helpersSettings,
            IUnitOfWork unitOfWork,
            ILogger<ProcessPlaylistItemJob> logger) {

            _unitOfWork = unitOfWork;
            _playlistRepository = playlistRepository;
            _uploadService = uploadService;
            _appSettings = appSettings.Value;
            _podcastRepository = podcastRepository;
            _storageSettings = storageSettings.Value;
            _imageStorageSettings = imageStorageSettings.Value;
            _helpersSettings = helpersSettings.Value;
            _logger = logger;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var items = await _playlistRepository.GetUnprocessedItems();
            foreach (var item in items) {
                await Execute(item.VideoId, item.Playlist.Id, null);
            }
            return true;
        }

        [Mutex("ProcessPlaylistItemJob")]
        public async Task<bool> Execute(string itemId, Guid playlistId, PerformContext context) {
            var item = await _playlistRepository.GetParsedItem(itemId, playlistId);
            if (item is null ||
                string.IsNullOrEmpty(item.VideoType) ||
                (!item.VideoType.Equals("youtube") && !item.VideoType.Equals("mixcloud")))
                return true;

            var url = item.VideoType.Equals("youtube") ? $"https://www.youtube.com/watch?v={item.VideoId}" :
                item.VideoType.Equals("mixcloud") ? $"https://mixcloud.com/{item.VideoId}" : string.Empty;
            if (string.IsNullOrEmpty(url)) {
                _logger.LogError($"Unknown video type for ParsedItem: {itemId} - {playlistId}");
            } else {
                var downloader = new AudioDownloader(url, _helpersSettings.Downloader);
                var info = downloader.GetInfo();
                if (info == AudioType.Valid) {
                    var podcast = await _podcastRepository.GetAsync(item.Playlist.PodcastId);
                    var uid = Guid.NewGuid();
                    var file = downloader.DownloadAudio(uid);

                    if (!File.Exists(file)) return true;

                    //we have the file so lets create the entry and ship to CDN
                    var entry = new PodcastEntry {
                        Title = downloader.Properties?.Title,
                        Description = downloader.Properties?.Description,
                        ProcessingStatus = ProcessingStatus.Uploading,
                        ImageUrl = downloader.Properties?.Thumbnail,
                        SourceUrl = url
                    };
                    podcast.PodcastEntries.Add(entry);
                    await _unitOfWork.CompleteAsync();

                    var uploaded = await _uploadService.UploadAudio(string.Empty, entry.Id, file);
                    if (!uploaded) return true;

                    item.IsProcessed = true;
                    await _unitOfWork.CompleteAsync();

                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.NotifyUser(entry.Podcast.AppUser.Id, "PodNoms",
                            $"{entry.Title} has finished processing",
                            entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                            entry.Podcast.GetThumbnailUrl(
                                _storageSettings.CdnUrl,
                                _imageStorageSettings.ContainerName)
                        ));

                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.SendCustomNotifications(entry.Podcast.Id, "PodNoms",
                            $"{entry.Title} has finished processing",
                            entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl)
                        ));
                } else {
                    _logger.LogError($"Processing playlist item {itemId} failed");
                    return false;
                }
            }

            return true;
        }
    }
}
