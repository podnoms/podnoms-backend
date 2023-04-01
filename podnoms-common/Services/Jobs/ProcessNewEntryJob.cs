using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.AudioParsing.Helpers;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Processor;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessNewEntryJob : AbstractHostedJob {
        private readonly IConfiguration _options;
        private readonly CachedAudioRetrievalService _audioRetriever;
        private readonly IRepoAccessor _repo;
        private readonly ILogger<ProcessNewEntryJob> _logger;
        private readonly AppSettings _appSettings;

        public ProcessNewEntryJob(
            ILogger<ProcessNewEntryJob> logger,
            IConfiguration options,
            IOptions<AppSettings> appSettings,
            CachedAudioRetrievalService audioRetriever,
            IRepoAccessor repo) : base(logger) {
            _options = options;
            _audioRetriever = audioRetriever;
            _repo = repo;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<bool> ProcessEntryFromUploadFile(
            Guid entryId, string audioUrl,
            string authToken, PerformContext context) {
            _setPerformContext(context);
            var entry = await _repo.Entries.GetAsync(entryId);
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
                job.Execute(entry.Id, localFile, null));

            return true;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task<bool> ProcessEntry(Guid entryId, PerformContext context) {
            var entry = await _repo.Entries.GetAsync(entryId);
            try {
                var localFile = PathUtils.GetScopedTempFile("mp3");

                var imageJobId = BackgroundJob.Enqueue<CacheRemoteImageJob>(
                    r => r.CacheImage(entry.Id));

                var extractJobId = BackgroundJob.Enqueue<IUrlProcessService>(
                    r => r.DownloadAudio(entry.Id, localFile));

                //TODO: Don't run this if IUrlProcessService fails
                var tagEntryJob = BackgroundJob.ContinueJobWith<TagEntryJob>(
                    extractJobId,
                    r => r.ExecuteForEntry(entry.Id, localFile, true, null));

                var uploadJobId = BackgroundJob.ContinueJobWith<IAudioUploadProcessService>(
                    tagEntryJob,
                    r => r.UploadAudio(entry.Id, localFile));

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
                return true;
            } catch (InvalidOperationException ex) {
                _logger.LogError($"Failed submitting job to processor\n{ex.Message}");
                context.WriteLine($"Failed submitting job to processor\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                await _repo.CompleteAsync();
                return false;
            }
        }

        public override Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }
    }
}
