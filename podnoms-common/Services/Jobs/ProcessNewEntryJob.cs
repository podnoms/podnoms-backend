using System;
using System.Threading.Tasks;
using Hangfire;
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
    public class ProcessNewEntryJob : IJob {
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

        public Task<bool> Execute() {
            throw new System.NotImplementedException();
        }

        public async Task<bool> ProcessEntry(Guid entryId, string authToken) {
            var entry = await _entryRepository.GetAsync(entryId);
            try {
                var imageJobId = BackgroundJob.Enqueue<CacheRemoteImageJob>(
                   r => r.CacheImage(entry.Id));
                var token = authToken.Replace("Bearer ", string.Empty);
                var extractJobId = BackgroundJob.Enqueue<IUrlProcessService>(
                    r => r.DownloadAudio(authToken, entry.Id));

                //TODO: Don't run this if IUrlProcessService fails
                var uploadJobId = BackgroundJob.ContinueJobWith<IAudioUploadProcessService>(
                    extractJobId, r => r.UploadAudio(authToken, entry.Id, entry.AudioUrl));

                var cdnUrl = _options.GetSection("StorageSettings")["CdnUrl"];
                var imageContainer = _options.GetSection("ImageFileStorageSettings")["ContainerName"];

                BackgroundJob.ContinueJobWith<INotifyJobCompleteService>(uploadJobId,
                    j => j.NotifyUser(token, entry.Podcast.AppUser.Id, "PodNoms",
                       $"{entry.Title} has finished processing",
                       entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                       entry.Podcast.GetThumbnailUrl(cdnUrl, imageContainer)
                   ));
                BackgroundJob.ContinueJobWith<INotifyJobCompleteService>(uploadJobId,
                    j => j.SendCustomNotifications(token, entry.Podcast.Id, "PodNoms",
                       $"{entry.Title} has finished processing",
                       entry.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl)
                    ));

                return true;
            } catch (InvalidOperationException ex) {
                _logger.LogError($"Failed submitting job to processor\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                await _unitOfWork.CompleteAsync();
                return false;
            }
        }
    }
}
