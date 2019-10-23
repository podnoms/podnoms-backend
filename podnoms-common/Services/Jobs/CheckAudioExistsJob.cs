using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Jobs {
    public class CheckAudioExistsJob : IHostedJob {
        public readonly IEntryRepository _entryRepository;
        public readonly StorageSettings _storageSettings;
        public readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<CheckAudioExistsJob> _logger;
        private readonly IMailSender _mailSender;

        public CheckAudioExistsJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILoggerFactory logger, IMailSender mailSender) {
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _entryRepository = entryRepository;

            _logger = logger.CreateLogger<CheckAudioExistsJob>();
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var unfound = new StringBuilder();
            var items = _entryRepository.GetAll();
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
                    unfound.Append($"Id: {item.Id} - AudioUrl: {item.AudioUrl} - SourceUrl: {item.SourceUrl}{Environment.NewLine}");
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
