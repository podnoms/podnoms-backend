using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Jobs {
    public class UploadAudioJob : IJob {
        private readonly IAudioUploadProcessService _uploadProcessService;
        private readonly CachedAudioRetrievalService _audioRetriever;
        private readonly ILogger _logger;

        public UploadAudioJob(
            IAudioUploadProcessService uploadProcessService,
            CachedAudioRetrievalService audioRetriever,
            ILogger<UploadAudioJob> logger, IMailSender mailSender) {
            _uploadProcessService = uploadProcessService;
            _audioRetriever = audioRetriever;
            _logger = logger;
        }

        public Task<bool> Execute() {
            throw new NotImplementedException();
        }

        public Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }

        public async Task<bool> Execute(
                string authToken, Guid entryId, string remoteUrl,
                string extension, PerformContext context) {
            context.WriteLine($"Starting Upload Job for {entryId} - {remoteUrl}");

            /// <summary>
            /// The API endpoint caches the file in wwwroot
            /// This will need to grab that before processing
            /// probaby sub-optimal but I'm open to suggestions
            /// </summary>


            var cacheFile = await _audioRetriever.RetrieveAudio(authToken, entryId, remoteUrl, extension);
            if (File.Exists(cacheFile)) {
                return await _uploadProcessService.UploadAudio(authToken, entryId, cacheFile);
            } else {
                context.WriteLine($"Failed to cache remote file {remoteUrl}");
                return false;
            }
        }
    }
}
