using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Common.Services.Storage {

    public class AzureFileUploader : IFileUploader {
        private readonly StorageSettings _settings;
        public AzureFileUploader(IOptions<StorageSettings> settings, ILoggerFactory logger) {
            _settings = settings.Value;
        }
        public async Task<string> UploadFile(string sourceFile, string containerName, string destinationFile,
        string contentType, Action<int, long> progressCallback) {
            var storageAccount = CloudStorageAccount.Parse(_settings.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference(destinationFile);
            blockBlob.Properties.ContentType = contentType;

            var blockSize = 256 * 1024;
            blockBlob.StreamWriteSizeInBytes = blockSize;
            var bytesToUpload = (new FileInfo(sourceFile)).Length;
            var fileSize = bytesToUpload;

            if (bytesToUpload < blockSize) {
                await blockBlob.UploadFromFileAsync(sourceFile);
            } else {
                var blockIds = new List<string>();
                var index = 1;
                long startPosition = 0;
                long bytesUploaded = 0;
                do {
                    var bytesToRead = Math.Min(blockSize, bytesToUpload);
                    var blobContents = new byte[bytesToRead];
                    using (var fs = new FileStream(sourceFile, FileMode.Open)) {
                        fs.Position = startPosition;
                        fs.Read(blobContents, 0, (int)bytesToRead);
                    }
                    var mre = new ManualResetEvent(false);
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString("d6")));
                    Console.WriteLine("Now uploading block # " + index.ToString("d6"));
                    blockIds.Add(blockId);
                    await blockBlob.PutBlockAsync(blockId, new MemoryStream(blobContents), null);
                    bytesUploaded += bytesToRead;
                    bytesToUpload -= bytesToRead;
                    startPosition += bytesToRead;
                    index++;
                    var percentComplete = (double)bytesUploaded / (double)fileSize;
                    Console.WriteLine("Percent complete = " + percentComplete.ToString("P"));
                    if (progressCallback != null) progressCallback((int)(percentComplete * 100), bytesToUpload);

                    mre.Set();
                    mre.WaitOne();
                }
                while (bytesToUpload > 0);
                Console.WriteLine("Now committing block list");
                await blockBlob.PutBlockListAsync(blockIds);
                blockBlob.Properties.ContentType = "audio/mpeg";
                await blockBlob.SetPropertiesAsync();

                Console.WriteLine("Blob uploaded completely.");
            }
            return $"{containerName}/{destinationFile}";
        }
    }
}