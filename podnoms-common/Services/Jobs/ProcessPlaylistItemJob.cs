﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.ErrorHandling;
using static PodNoms.Common.Services.Processor.EntryPreProcessor;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistItemJob : AbstractHostedJob {
        private readonly AudioDownloader _audioDownloader;
        private readonly IRepoAccessor _repo;
        private readonly IRepoAccessor _repoAccessor;
        private readonly IUrlProcessService _processor;
        private readonly EntryPreProcessor _preProcessor;

        public ProcessPlaylistItemJob(
            IRepoAccessor repo,
            IRepoAccessor repoAccessor,
            IUrlProcessService processor,
            EntryPreProcessor preProcessor,
            AudioDownloader audioDownloader,
            ILogger<ProcessPlaylistItemJob> logger) : base(logger) {
            _repo = repo;
            _repoAccessor = repoAccessor;
            _processor = processor;
            _preProcessor = preProcessor;
            _audioDownloader = audioDownloader;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            return await Task.Factory.StartNew(() => true);
        }

        // [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 2)]
        [MaximumConcurrentExecutions(3)]
        public async Task<bool> Execute(ParsedItemResult item, Guid playlistId, PerformContext context) {
            _setContext(context);
            if (item is null || string.IsNullOrEmpty(item.VideoType)) {
                return false;
            }

            Log($"Starting process item:\n\t{item.Id}\n\t{item.Title}\n\thttps://www.youtube.com/watch?v={item.Id}");

            var playlist = await _repo.Playlists.GetAsync(playlistId);
            var url = item.VideoType.ToLower().Equals("youtube") ? $"https://www.youtube.com/watch?v={item.Id}" :
                item.VideoType.Equals("mixcloud") ? Flurl.Url.Combine($"https://mixcloud.com/", item.Id) :
                string.Empty;
            if (string.IsNullOrEmpty(url)) {
                LogError($"Unknown video type for ParsedItem: {item.Id} - {playlist.Id}");
            } else {
                Log($"Getting info");
                var info = await _audioDownloader.GetInfo(url, playlist.Podcast.AppUserId);
                if (info != RemoteUrlType.Invalid) {
                    Log($"URL is valid");

                    var podcast = await _repo.Podcasts.GetAsync(playlist.PodcastId);
                    Log("Downloading audio");
                    try {
                        var entry = new PodcastEntry {
                            SourceUrl = url,
                            SourceItemId = item.Id,
                            SourceCreateDate = item.UploadDate,
                            ProcessingStatus = ProcessingStatus.Uploading,
                            Playlist = playlist,
                            Podcast = podcast,
                        };
                        await _processor.GetInformation(entry, podcast.AppUserId);
                        podcast.PodcastEntries.Add(entry);
                        await _repoAccessor.CompleteAsync();
                        var result = await _preProcessor.PreProcessEntry(podcast.AppUser, entry);
                        return result == EntryProcessResult.Succeeded;
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
