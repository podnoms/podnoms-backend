using System;
using System.IO;
using System.Net.Http;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public UploadAudioJob(
            IAudioUploadProcessService uploadProcessService,
            IHttpClientFactory httpClientFactory,
            ILogger<UploadAudioJob> logger, IMailSender mailSender) {
            _uploadProcessService = uploadProcessService;
            _httpClientFactory = httpClientFactory;
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
            string cacheFile = Path.Combine(
                Path.GetTempPath(),
                $"{System.Guid.NewGuid().ToString()}.{extension}"
            );
            using (var client = _httpClientFactory.CreateClient("podnoms")) {
                using (HttpResponseMessage response = await client.GetAsync(remoteUrl)) {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                        using (Stream streamToWriteTo = File.Open(cacheFile, FileMode.Create)) {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                        response.Content = null;
                    }
                }
            }
            if (File.Exists(cacheFile)) {
                return await _uploadProcessService.UploadAudio(authToken, entryId, cacheFile);
            } else {
                context.WriteLine($"Failed to cache remote file {remoteUrl}");
                return false;
            }
        }
    }
}
