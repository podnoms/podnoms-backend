using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
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
    public class ProcessPlaylistsJob : IJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly HelpersSettings _helpersSettings;
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ProcessPlaylistsJob> _logger;
        private readonly YouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPlaylistsJob (
            IPlaylistRepository playlistRepository,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<AppSettings> appSettings,
            ILoggerFactory logger,

            YouTubeParser youTubeParser,
            MixcloudParser mixcloudParser) {
            _unitOfWork = unitOfWork;
            _youTubeParser = youTubeParser;
            _mixcloudParser = mixcloudParser;
            _playlistRepository = playlistRepository;
            _entryRepository = entryRepository;
            _helpersSettings = helpersSettings.Value;
            _storageSettings = storageSettings.Value;
            _appSettings = appSettings.Value;
            _logger = logger.CreateLogger<ProcessPlaylistsJob> ();
        }

        public async Task<bool> Execute () {
            return true;
            var playlists = _playlistRepository.GetAll ();
            foreach (var playlist in playlists) {
                await Execute (playlist.Id);
            }
            return true;
        }
        public async Task<bool> Execute (Guid playlistId) {
            return true;
            try {
                var playlist = await _playlistRepository.GetAsync (playlistId);
                if (playlist is null)
                    return false;

                //first check quotas
                var quota = playlist.Podcast.AppUser.DiskQuota ?? _storageSettings.DefaultUserQuota;
                var totalUsed = (await _entryRepository.GetAllForUserAsync (playlist.Podcast.AppUser.Id))
                    .Select (x => x.AudioFileSize)
                    .Sum ();

                if (totalUsed >= quota) {
                    _logger.LogError ($"Storage quota exceeded for {playlist.Podcast.AppUser.FullName}");
                    BackgroundJob.Enqueue<INotifyJobCompleteService> (
                        service => service.SendCustomNotifications (playlist.Podcast.Id, "PodNoms",
                            $"Failure processing playlist\n{playlist.Podcast.Title}\n" +
                            $"Your have exceeded your storage quota of {quota.Bytes().ToString()}",
                            playlist.Podcast.GetAuthenticatedUrl (_appSettings.SiteUrl)
                        ));
                    return false;
                }

                //check for active subscription
                if (playlist.PodcastEntries.Count >= _storageSettings.DefaultEntryCount) {
                    _logger.LogError ($"Entry count exceeded for {playlist.Podcast.AppUser.FullName}");
                    BackgroundJob.Enqueue<INotifyJobCompleteService> (
                        service => service.SendCustomNotifications (playlist.Podcast.Id, "PodNoms",
                            $"Failure processing playlist\n{playlist.Podcast.Title}\n" +
                            $"Your quota of {_storageSettings.DefaultEntryCount} items per playlist has been exceeded",
                            playlist.Podcast.GetAuthenticatedUrl (_appSettings.SiteUrl)
                        ));
                    return false;
                }

                var resultList = new List<ParsedItemResult> ();
                var downloader = new AudioDownloader (playlist.SourceUrl, _helpersSettings.Downloader);
                var info = downloader.GetInfo ();
                var id = ((PlaylistDownloadInfo) downloader.RawProperties)?.Id;

                if (string.IsNullOrEmpty (id)) return true;

                if (YouTubeParser.ValidateUrl (playlist.SourceUrl)) {
                    var searchTerm = (playlist.SourceUrl.Contains ("/user/")) ? "forUsername" : "id";
                    resultList = await _youTubeParser.GetPlaylistEntriesForId (id);
                    //make sure the items are sorted in ascending date order
                    //so they will be processed in the order they were created
                } else if (MixcloudParser.ValidateUrl (playlist.SourceUrl)) {
                    resultList = await _mixcloudParser.GetEntries (playlist.SourceUrl);
                }

                if (resultList is null) return true;
                //order in reverse so the newest item is added first
                foreach (var item in resultList?.OrderBy (r => r.UploadDate)) {
                    _logger.LogDebug ($"Processing playlist item: {item.Id}");
                    if (playlist.ParsedPlaylistItems.Any (p => p.VideoId == item.Id)) continue;

                    _logger.LogDebug ($"Found missing item: {item.Id}");
                    playlist.ParsedPlaylistItems.Add (new ParsedPlaylistItem {
                        VideoId = item.Id,
                            VideoType = item.VideoType
                    });
                    await _unitOfWork.CompleteAsync ();
                    BackgroundJob.Enqueue<ProcessPlaylistItemJob> (service => service.Execute (item.Id, playlist.Id));
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError (ex.Message);
            }
            return false;
        }
    }
}
