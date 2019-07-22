using System;
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
        private readonly ILogger _logger;

        public UploadAudioJob(
            IAudioUploadProcessService uploadProcessService,
            ILogger<UploadAudioJob> logger, IMailSender mailSender) {
            _uploadProcessService = uploadProcessService;
            _logger = logger;
        }

        public Task<bool> Execute() {
            throw new NotImplementedException();
        }

        public Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }

        public async Task<bool> Execute(string authToken, Guid entryId, string localFile, PerformContext context) {
            context.WriteLine($"Starting Upload Job for {entryId} - {localFile}");
            return await _uploadProcessService.UploadAudio(authToken, entryId, localFile);
        }
    }
}
