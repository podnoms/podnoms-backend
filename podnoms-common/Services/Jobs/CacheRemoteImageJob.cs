using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class CacheRemoteImageJob : IJob {
        private readonly IEntryRepository _entryRepository;
        private readonly IFileUploader _fileUploader;
        private readonly IUnitOfWork _unitOfWork;
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _imageFileStoragesSettings;
        private readonly ILogger _logger;

        public CacheRemoteImageJob(IEntryRepository entryRepository,
            IFileUploader fileUploader,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStoragesSettings,
            IUnitOfWork unitOfWork,
            ILoggerFactory logger) {
            _entryRepository = entryRepository;
            _fileUploader = fileUploader;
            _unitOfWork = unitOfWork;
            _storageSettings = storageSettings.Value;
            _imageFileStoragesSettings = imageFileStoragesSettings.Value;
            _logger = logger.CreateLogger<CacheRemoteImageJob>();
        }

        public async Task<bool> Execute() {
            _logger.LogDebug("Start processing images");
            var uncached = _entryRepository
                .GetAll()
                .Where(e => e.ImageUrl.StartsWith("http"));

            foreach (var e in uncached) {
                _logger.LogDebug($"Process image for: {e.ImageUrl}");
                var (original, thumbnail) = await CacheImage(e.ImageUrl, e.Id.ToString());
                if (!string.IsNullOrEmpty(original) && !string.IsNullOrEmpty(thumbnail)) {
                    _logger.LogDebug($"Successfully processed: {original}");
                    e.ImageUrl = original;
                }
            }
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<(string, string)> CacheImage(Guid entryId) {
            var entry = await _entryRepository.GetAsync(entryId);
            if (entry == null) return (string.Empty, string.Empty);

            var (original, thumbnail) = await CacheImage(entry.ImageUrl, entry.Id.ToString());

            if (string.IsNullOrEmpty(original)) return (string.Empty, string.Empty);

            entry.ImageUrl = original;
            await _unitOfWork.CompleteAsync();

            return (original, thumbnail);
        }

        public async Task<(string, string)> CacheImage(string imageUrl, string destUid) {
            try {
                var sourceFile = await HttpUtils.DownloadFile(imageUrl);
                var thumbnailFile = ImageUtils.CreateThumbnail(sourceFile, destUid, 32, 32);

                var original = await _fileUploader.UploadFile(
                    sourceFile,
                    _imageFileStoragesSettings.ContainerName,
                    $"entry/{destUid}.png",
                    "image/png", null);
                var thumbnail = await _fileUploader.UploadFile(
                    thumbnailFile,
                    _imageFileStoragesSettings.ContainerName,
                    $"entry/{destUid}-32x32.png",
                    "image/png", null);

                return (original, thumbnail);
            } catch (Exception ex) {
                _logger.LogError($"Error caching image: {ex.Message}");
            }

            return (string.Empty, string.Empty);
        }
    }
}