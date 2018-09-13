using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
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
        private readonly HelpersSettings _helpersSettings;
        private readonly ILogger<ProcessPlaylistsJob> _logger;
        private readonly YouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPlaylistsJob(
                IPlaylistRepository playlistRepository,
                IUnitOfWork unitOfWork,
                IOptions<HelpersSettings> helpersSettings,
                ILoggerFactory logger,
                YouTubeParser youTubeParser,
                MixcloudParser mixcloudParser) {
            _unitOfWork = unitOfWork;
            _youTubeParser = youTubeParser;
            _mixcloudParser = mixcloudParser;
            _playlistRepository = playlistRepository;
            _helpersSettings = helpersSettings.Value;
            _logger = logger.CreateLogger<ProcessPlaylistsJob>();
        }

        public async Task<bool> Execute() {
            var playlists = _playlistRepository.GetAll()
                .ToList();

            foreach (var playlist in playlists) {
                await Execute(playlist.Id);
            }
            return true;
        }
        public async Task<bool> Execute(Guid playlistId) {
            try {
                var playlist = await _playlistRepository.GetAsync(playlistId);
                if (playlist == null)
                    return false;
                var resultList = new List<ParsedItemResult>();

                var downloader = new AudioDownloader(playlist.SourceUrl, _helpersSettings.Downloader);
                var info = downloader.GetInfo();
                var id = ((PlaylistDownloadInfo)downloader.RawProperties)?.Id;

                if (string.IsNullOrEmpty(id)) return true;

                if (YouTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    var searchTerm = (playlist.SourceUrl.Contains("/user/")) ? "forUsername" : "id";
                    resultList = await _youTubeParser.GetPlaylistEntriesForId(id);
                    //make sure the items are sorted in ascending date order
                    //so they will be processed in the order they were created
                } else if (MixcloudParser.ValidateUrl(playlist.SourceUrl)) {
                    resultList = await _mixcloudParser.GetEntries(playlist.SourceUrl);
                }

                if (resultList == null) return true;
                //order in reverse so the newest item is added first
                foreach (var item in resultList?.OrderBy(r => r.UploadDate)) {
                    if (playlist.ParsedPlaylistItems.Any(p => p.VideoId == item.Id)) continue;
                    playlist.ParsedPlaylistItems.Add(new ParsedPlaylistItem
                    {
                        VideoId = item.Id,
                        VideoType = item.VideoType
                    });
                    await _unitOfWork.CompleteAsync();
                    BackgroundJob.Enqueue<ProcessPlaylistItemJob>(service => service.ExecuteForItem(item.Id, playlist.Id));
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
            }
            return false;
        }
    }
}