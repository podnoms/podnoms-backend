using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistItemJob : IHostedJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly AppSettings _appSettings;
        private readonly IPodcastRepository _podcastRepository;
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _imageStorageSettings;
        private readonly HelpersSettings _helpersSettings;
        private readonly AudioDownloader _audioDownloader;
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
            AudioDownloader audioDownloader,
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
            _audioDownloader = audioDownloader;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            return true;
        }

        [Mutex("ProcessPlaylistItemJob")]
        // public async Task<bool> Execute(string itemId, Guid playlistId, PerformContext context) {
        public async Task<bool> Execute(ParsedItemResult item, Guid playlistId, PerformContext context) {
            if (item is null || string.IsNullOrEmpty(item.VideoType)) {
                return false;
            }

            var playlist = await _playlistRepository.GetAsync(playlistId);
            var url = item.VideoType.ToLower().Equals("youtube") ?
                $"https://www.youtube.com/watch?v={item.Id}" :
                item.VideoType.Equals("mixcloud") ?
                    $"https://mixcloud.com/{item.Id}" :
                    string.Empty;
            if (string.IsNullOrEmpty(url)) {
                _logger.LogError($"Unknown video type for ParsedItem: {item.Id} - {playlist.Id}");
            } else {
                var info = await _audioDownloader.GetInfo(url);
                if (info != RemoteUrlType.Invalid) {
                    var podcast = await _podcastRepository.GetAsync(playlist.PodcastId);
                    var uid = Guid.NewGuid();
                    var file = _audioDownloader.DownloadAudio(uid, url);

                    if (!File.Exists(file)) return true;

                    //we have the file so lets create the entry and ship to CDN
                    var entry = new PodcastEntry {
                        Title = _audioDownloader.Properties?.Title,
                        Description = _audioDownloader.Properties?.Description,
                        ProcessingStatus = ProcessingStatus.Uploading,
                        ImageUrl = _audioDownloader.Properties?.Thumbnail,
                        SourceCreateDate = item.UploadDate,
                        SourceItemId = item.Id,
                        SourceUrl = url
                    };
                    podcast.PodcastEntries.Add(entry);
                    playlist.PodcastEntries.Add(entry);
                    await _unitOfWork.CompleteAsync();

                    var uploaded = await _uploadService.UploadAudio(string.Empty, entry.Id, file);
                    if (!uploaded) {
                        entry.ProcessingStatus = ProcessingStatus.Failed;
                        await _unitOfWork.CompleteAsync();
                        return true;
                    }

                    entry.ProcessingStatus = ProcessingStatus.Processed;
                    entry.Processed = true;
                    await _unitOfWork.CompleteAsync();

                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.NotifyUser(entry.Podcast.AppUser.Id, "PodNoms",
                            $"{entry.Title} has finished processing",
                            entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                            entry.Podcast.GetThumbnailUrl(
                                _storageSettings.CdnUrl,
                                _imageStorageSettings.ContainerName),
                            NotificationOptions.NewPlaylistEpisode
                        ));

                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.SendCustomNotifications(
                            entry.Podcast.Id,
                            entry.Podcast.AppUser.GetBestGuessName(),
                            "PodNoms",
                            $"{entry.Title} has finished processing",
                            entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl)
                        ));
                } else {
                    _logger.LogError($"Processing playlist item {item.Id} failed");
                    return false;
                }
            }

            return true;
        }
    }
}
