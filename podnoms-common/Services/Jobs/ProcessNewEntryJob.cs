﻿using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Caching;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessNewEntryJob : AbstractHostedJob {
        private readonly IConfiguration _options;
        private readonly IEntryRepository _entryRepository;
        private readonly IResponseCacheService _cache;
        private readonly CachedAudioRetrievalService _audioRetriever;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessNewEntryJob> _logger;
        private readonly AppSettings _appSettings;

        public ProcessNewEntryJob(
            ILogger<ProcessNewEntryJob> logger,
            IConfiguration options,
            IEntryRepository entryRepository,
            IOptions<AppSettings> appSettings,
            IResponseCacheService cache,
            CachedAudioRetrievalService audioRetriever,
            IUnitOfWork unitOfWork) : base(logger) {
            _options = options;
            _entryRepository = entryRepository;
            _cache = cache;
            _audioRetriever = audioRetriever;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<bool> ProcessEntryFromUploadFile(Guid entryId, string audioUrl,
            string authToken, PerformContext context) {
            _setContext(context);
            var entry = await _entryRepository.GetAsync(entryId);
            var remoteUrl = Flurl.Url.Combine(_appSettings.ApiUrl, audioUrl);

            Log($"Caching: {remoteUrl}");
            var localFile = await _audioRetriever.RetrieveAudio(
                authToken,
                entry.Id, remoteUrl, "mp3");

            Log($"Submitting waveform job for {localFile}");
            var waveFormJobId = BackgroundJob.Enqueue<GenerateWaveformsJob>(job =>
                job.ExecuteForEntry(entry.Id, localFile, null));

            Log($"Submitting tag entry job for {localFile}");
            var tagEntryJob = BackgroundJob.ContinueJobWith<TagEntryJob>(
                waveFormJobId,
                r => r.ExecuteForEntry(entry.Id, localFile, true, null));

            Log($"Submitting upload job for {localFile}");
            var tagEntryJobId = BackgroundJob.ContinueJobWith<UploadAudioJob>(tagEntryJob, job =>
                job.Execute(authToken, entry.Id, localFile, null));

            await _cache.InvalidateCacheResponseAsync(
                $"podcast____{entry.Podcast.AppUser.Slug}__rss__{entry.Podcast.Slug}");
            return true;
        }

        public async Task<bool> ProcessEntry(Guid entryId, string authToken, PerformContext context) {
            var entry = await _entryRepository.GetAsync(entryId);
            try {
                var localFile = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3");
                var imageJobId = BackgroundJob.Enqueue<CacheRemoteImageJob>(
                    r => r.CacheImage(entry.Id));

                var token = authToken.Replace("Bearer ", string.Empty);
                var extractJobId = BackgroundJob.Enqueue<IUrlProcessService>(
                    r => r.DownloadAudio(authToken, entry.Id, localFile));

                //TODO: Don't run this if IUrlProcessService fails
                var tagEntryJob = BackgroundJob.ContinueJobWith<TagEntryJob>(
                    extractJobId,
                    r => r.ExecuteForEntry(entry.Id, localFile, true, null));

                var uploadJobId = BackgroundJob.ContinueJobWith<IAudioUploadProcessService>(
                    tagEntryJob,
                    r => r.UploadAudio(authToken, entry.Id, localFile));

                //if we wait until everything is done, we can delete the local file
                var waveformJobId = BackgroundJob.ContinueJobWith<GenerateWaveformsJob>(
                    extractJobId,
                    r => r.ExecuteForEntry(entry.Id, localFile, null));

                var cdnUrl = _options.GetSection("StorageSettings")["CdnUrl"];
                var imageContainer = _options.GetSection("ImageFileStorageSettings")["ContainerName"];

                context.WriteLine($"Submitting notify events");

                BackgroundJob.ContinueJobWith<INotifyJobCompleteService>(uploadJobId,
                    j => j.NotifyUser(
                        entry.Podcast.AppUser.Id,
                        "New Podcast Entry Available",
                        $"{entry.Title} has finished processing",
                        entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                        entry.Podcast.GetThumbnailUrl(cdnUrl, imageContainer),
                        NotificationOptions.UploadCompleted
                    ));
                BackgroundJob.ContinueJobWith<INotifyJobCompleteService>(uploadJobId,
                    j => j.SendCustomNotifications(
                        entry.Podcast.Id,
                        entry.Podcast.AppUser.GetBestGuessName(),
                        "PodNoms",
                        $"{entry.Title} has finished processing",
                        entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl)
                    ));
                
                await _cache.InvalidateCacheResponseAsync(
                    $"podcast____{entry.Podcast.AppUser.Slug}__rss__{entry.Podcast.Slug}");
                return true;
            } catch (InvalidOperationException ex) {
                _logger.LogError($"Failed submitting job to processor\n{ex.Message}");
                context.WriteLine($"Failed submitting job to processor\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                await _unitOfWork.CompleteAsync();
                return false;
            }
        }

        public override Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }
    }
}
