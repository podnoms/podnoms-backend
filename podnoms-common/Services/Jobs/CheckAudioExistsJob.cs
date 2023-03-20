using System;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Jobs {
    public class CheckAudioExistsJob : IHostedJob {
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<CheckAudioExistsJob> _logger;
        private readonly IRepoAccessor _repo;
        private readonly IMailSender _mailSender;

        public CheckAudioExistsJob(IRepoAccessor repo, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILoggerFactory logger, IMailSender mailSender) {
            _repo = repo;
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;

            _logger = logger.CreateLogger<CheckAudioExistsJob>();
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var unfound = new StringBuilder();
            var items = _repo.Entries.GetAll();
            var storageAccount = CloudStorageAccount.Parse(_storageSettings.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_audioStorageSettings.ContainerName);
            foreach (var item in items) {
                var key = $"{item.Id}.mp3";
                _logger.LogDebug($"Checking for: {key}");
                var exists = await container
                    .GetBlockBlobReference(key)
                    .ExistsAsync();
                if (!exists) {
                    //check for audio in backup container
                    if (!string.IsNullOrEmpty(item.AudioUrl)) {
                        var backedUp = item.AudioUrl.Replace("audio/", "backup/");
                        if (await container
                                .GetBlockBlobReference(backedUp)
                                .ExistsAsync()) {
                            await container.RenameAsync(backedUp, key);
                            continue;
                        }

                        //check for one in audio container where filename doesn't match <id>.mp3
                        var oldUrl = item.AudioUrl.Replace("audio/", "");
                        if (await container
                                .GetBlobReference(oldUrl)
                                .ExistsAsync()) {
                            await container.RenameAsync(oldUrl, key);
                            continue;
                        }
                    }

                    unfound.Append(
                        $"Id: {item.Id} - AudioUrl: {item.AudioUrl} - SourceUrl: {item.SourceUrl}{Environment.NewLine}");
                }
            }

            await _mailSender.SendEmailAsync(
                "fergal.moran+podnoms@gmail.com",
                "Missing audio found",
                new MailDropin {
                    username = "Fergal Moran",
                    title = "Missing audio found",
                    message = unfound.ToString()
                });
            return true;
        }
    }
}
