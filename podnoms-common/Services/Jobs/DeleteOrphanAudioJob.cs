using System;
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
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Jobs {
    public class DeleteOrphanAudioJob : AbstractHostedJob {
        private readonly IEntryRepository _entryRepository;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioStorageSettings;

        public DeleteOrphanAudioJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILogger<DeleteOrphanAudioJob> logger) : base(logger) {
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _entryRepository = entryRepository;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            try {
                var storageAccount = CloudStorageAccount.Parse(_storageSettings.ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_audioStorageSettings.ContainerName);
                short deletedCount = 0;
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
                            if (blob.Name.Contains("backup/")) {
                                continue;
                            }

                            var url = $"{_audioStorageSettings.ContainerName}/{blob.Name}";
                            var id = blob.Name.Split('.').First();

                            var entry = await _entryRepository.GetAsync(Guid.Parse(id));
                            if (entry is null) {
                                await container.RenameAsync(blob.Name, $"backup/{blob.Name}");
                                //await blob.DeleteIfExistsAsync();
                                deletedCount++;
                            }
                            blobCount++;
                            Log($"{id} : {blobCount} processed, {deletedCount} deleted.");
                        } catch (Exception e) {
                            LogWarning($"Error processing blob {blob.Uri}\n{e.Message}");
                        }
                    }

                    continuationToken = blobs.ContinuationToken;
                } while (continuationToken != null);

                Log($"Successfully processed orphans, {blobCount} visited, {deletedCount} deleted.");
                return true;
            } catch (Exception ex) {
                LogError($"Error clearing orphans\n{ex.Message}");
            }

            return false;
        }
    }
}
