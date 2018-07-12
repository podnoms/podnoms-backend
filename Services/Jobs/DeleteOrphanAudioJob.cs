using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Settings;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Jobs {
    public class DeleteOrphanAudioJob : IJob {
        public readonly IEntryRepository _entryRepository;
        public readonly StorageSettings _storageSettings;
        public readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<DeleteOrphanAudioJob> _logger;
        private readonly IMailSender _mailSender;

        public DeleteOrphanAudioJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILoggerFactory logger, IMailSender mailSender) {
            this._mailSender = mailSender;
            this._storageSettings = storageSettings.Value;
            this._audioStorageSettings = audioStorageSettings.Value;
            this._entryRepository = entryRepository;

            this._logger = logger.CreateLogger<DeleteOrphanAudioJob>();
        }

        public async Task<bool> Execute() {
            try {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageSettings.ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_audioStorageSettings.ContainerName);
                short blobCount = 0;
                var blobs = await container.ListBlobsSegmentedAsync(null);
                foreach (CloudBlockBlob blob in blobs.Results) {
                    try {
                        Console.WriteLine(blob.StorageUri);
                        var url = $"{_audioStorageSettings.ContainerName}/{blob.Name}";
                        var entry = _entryRepository.GetAll()
                            .Where(r => r.AudioUrl == url);
                        if (entry == null) {
                            await blob.DeleteIfExistsAsync();
                            blobCount++;
                        }
                    } catch (Exception e) {
                        _logger.LogWarning($"Error processing blob {blob.Uri}\n{e.Message}");
                    }
                }
                await this._mailSender.SendEmailAsync("fergal.moran@gmail.com", $"DeleteOrphanAudioJob: Complete {blobCount}", string.Empty);
                return true;
            } catch (Exception ex) {
                _logger.LogError($"Error clearing orphans\n{ex.Message}");
            }
            return false;
        }
    }
}