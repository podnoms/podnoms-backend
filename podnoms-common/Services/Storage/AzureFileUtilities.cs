using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Common.Services.Storage {
    public class AzureFileUtilities : IFileUtilities {
        private readonly StorageSettings _settings;
        public AzureFileUtilities (IOptions<StorageSettings> settings) {
            _settings = settings.Value;
        }
        private async Task<CloudBlobContainer> _getContainer (string containerName, bool create = false) {
            var storageAccount = CloudStorageAccount.Parse (_settings.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient ();
            var container = blobClient.GetContainerReference (containerName);
            var exists = await container.ExistsAsync ();
            if (!exists && !create) {
                throw new InvalidOperationException ($"Container ${container} does not exist");
            }

            return container;
        }
        public async Task<long> GetRemoteFileSize (string containerName, string fileName) {
            try {
                var container = await _getContainer (containerName);
                var blob = container.GetBlockBlobReference (fileName);
                await blob.FetchAttributesAsync ();
                return blob.Properties.Length;
            } catch (InvalidOperationException ex) {
                throw ex;
            }
        }
        public async Task<bool> CheckFileExists (string containerName, string fileName) {
            try {
                var container = await _getContainer (containerName);
                var blob = container.GetBlockBlobReference (fileName);
                await blob.FetchAttributesAsync ();
                return true;
            } catch (Exception) {
                return false;
            }
        }
        public async Task<bool> CopyRemoteFile (string sourceContainerName, string sourceFile,
            string destinationContainerName, string destinationFile) {
            try {
                var sourceContainer = await _getContainer (sourceContainerName);
                var destinationContainer = await _getContainer (destinationContainerName, true);
                var sourceBlob = sourceContainer.GetBlockBlobReference (sourceFile);
                var destBlob = destinationContainer.GetBlockBlobReference (destinationFile);
                if (await sourceBlob.ExistsAsync ()) {
                    await destBlob.StartCopyAsync (sourceBlob);

                    return true;
                }
                return false;
            } catch (InvalidOperationException ex) {
                throw ex;
            }
        }
    }
}