﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Common.Services.Jobs {
    public class DeleteOrphanAudioJob : IJob {
        public readonly IEntryRepository _entryRepository;
        public readonly StorageSettings _storageSettings;
        public readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<DeleteOrphanAudioJob> _logger;
        private readonly IMailSender _mailSender;

        public DeleteOrphanAudioJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILoggerFactory logger, IMailSender mailSender) {
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _entryRepository = entryRepository;

            _logger = logger.CreateLogger<DeleteOrphanAudioJob>();
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            try {
                var storageAccount = CloudStorageAccount.Parse(_storageSettings.ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_audioStorageSettings.ContainerName);
                short blobCount = 0;
                BlobContinuationToken continuationToken = null;
                do {
                    var blobs = await container.ListBlobsSegmentedAsync(
                        prefix: null,
                        useFlatBlobListing: true,
                        blobListingDetails: BlobListingDetails.None,
                        maxResults: 1000,
                        currentToken: continuationToken,
                        options: null,
                        operationContext: null);
                    foreach (CloudBlockBlob blob in blobs.Results) {
                        try {
                            Console.WriteLine(blob.StorageUri);
                            var url = $"{_audioStorageSettings.ContainerName}/{blob.Name}";
                            var entry = _entryRepository.GetAll()
                                .Where(r => r.AudioUrl == url);
                            if (entry is null) {
                                await blob.DeleteIfExistsAsync();
                                blobCount++;
                            }
                        } catch (Exception e) {
                            _logger.LogWarning($"Error processing blob {blob.Uri}\n{e.Message}");
                        }
                    }
                    continuationToken = blobs.ContinuationToken;
                } while (continuationToken != null);
                await _mailSender.SendEmailAsync(
                    "fergal.moran@gmail.com",
                    $"DeleteOrphanAudioJob: Complete {blobCount}",
                    string.Empty);
                return true;
            } catch (Exception ex) {
                _logger.LogError($"Error clearing orphans\n{ex.Message}");
            }
            return false;
        }
    }
}
