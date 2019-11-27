using System;
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
using PodNoms.Common.Services.Processor;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessNewEntryJob : IHostedJob {
        private readonly IConfiguration _options;
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessNewEntryJob> _logger;
        private readonly AppSettings _appSettings;

        public ProcessNewEntryJob(
            IConfiguration options,
            IEntryRepository entryRepository,
            IOptions<AppSettings> appSettings,
            IUnitOfWork unitOfWork,
            ILogger<ProcessNewEntryJob> logger) {
            _options = options;
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _appSettings = appSettings.Value;
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            return await Task.Run(() => false);
        }

        public async Task<bool> ProcessEntry(Guid entryId, string authToken, PerformContext context) {
            var entry = await _entryRepository.GetAsync(entryId);
            try {
                var localFile =  Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3");
                var imageJobId = BackgroundJob.Enqueue<CacheRemoteImageJob>(
                   r => r.CacheImage(entry.Id));

                var token = authToken.Replace("Bearer ", string.Empty);
                var extractJobId = BackgroundJob.Enqueue<IUrlProcessService>(
                    r => r.DownloadAudio(authToken, entry.Id, localFile));

                //TODO: Don't run this if IUrlProcessService fails
                var uploadJobId = BackgroundJob.ContinueJobWith<IAudioUploadProcessService>(
                    extractJobId,
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

                return true;
            } catch (InvalidOperationException ex) {
                _logger.LogError($"Failed submitting job to processor\n{ex.Message}");
                context.WriteLine($"Failed submitting job to processor\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                await _unitOfWork.CompleteAsync();
                return false;
            }
        }
    }
}
