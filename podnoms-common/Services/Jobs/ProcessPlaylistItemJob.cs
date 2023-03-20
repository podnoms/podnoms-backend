using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using System;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.Common.Persistence.Repositories;
using static PodNoms.Common.Services.Processor.EntryPreProcessor;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistItemJob : AbstractHostedJob {
        private readonly AudioDownloader _audioDownloader;
        private readonly IRepoAccessor _repo;
        private readonly IUrlProcessService _processor;
        private readonly EntryPreProcessor _preProcessor;

        public ProcessPlaylistItemJob(
            IRepoAccessor repo,
            IUrlProcessService processor,
            EntryPreProcessor preProcessor,
            AudioDownloader audioDownloader,
            ILogger<ProcessPlaylistItemJob> logger) : base(logger) {
            _repo = repo;
            _processor = processor;
            _preProcessor = preProcessor;
            _audioDownloader = audioDownloader;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            _setPerformContext(context);
            return await Task.Factory.StartNew(() => true);
        }

        // [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 2)]
        [MaximumConcurrentExecutions(3)]
        public async Task<bool> Execute(ParsedItemResult item, Guid playlistId, PerformContext performContext) {
            _setPerformContext(performContext);
            if (item is null || string.IsNullOrEmpty(item.VideoType)) {
                return false;
            }

            var playlist = await _repo.Playlists.GetAsync(playlistId);
            var url = item.VideoType.ToLower().Equals("youtube") ? $"https://www.youtube.com/watch?v={item.Id}" :
                item.VideoType.Equals("mixcloud") ? Flurl.Url.Combine($"https://mixcloud.com/", item.Id) :
                string.Empty;
            if (string.IsNullOrEmpty(url)) {
                LogError($"Unknown video type for ParsedItem: {item.Id} - {playlist.Id}");
            } else {
                var info = await _audioDownloader.GetInfo(url, playlist.Podcast.AppUserId);
                if (info != RemoteUrlType.Invalid) {
                    var podcast = await _repo.Podcasts.GetAsync(playlist.PodcastId);
                    try {
                        var entry = new PodcastEntry {
                            SourceUrl = url,
                            SourceItemId = item.Id,
                            SourceCreateDate = item.UploadDate,
                            ProcessingStatus = ProcessingStatus.Uploading,
                            Playlist = playlist,
                            PodcastId = podcast.Id,
                        };
                        await _processor.GetInformation(entry, podcast.AppUserId);
                        await _repo.Entries.AddOrUpdate(entry, t => t.SourceItemId.Equals(item.Id));
                        await _repo.CompleteAsync();
                        BackgroundJob.Enqueue<ProcessNewEntryJob>(e => e.ProcessEntry(entry.Id, null));
                        return true;
                    } catch (AudioDownloadException e) {
                        //TODO: we should mark this as failed
                        //so we don't continuously process it
                        LogError(e.Message);
                    }
                } else {
                    LogError($"Processing playlist item {item.Id} failed");
                    return false;
                }
            }

            return true;
        }
    }
}
