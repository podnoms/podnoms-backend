using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        public CacheRemoteImageJob (IEntryRepository entryRepository,
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
            _logger = logger.CreateLogger<CacheRemoteImageJob> ();
        }

        public async Task<bool> Execute () {
            var images = _entryRepository
                .GetAll ();

            int i = 1;
            int count = images.Count ();
            foreach (var e in images) {
                _logger.LogDebug ($"Caching image for: {e.Id}");
                var exists = await _fileUploader.FileExists (_imageFileStoragesSettings.ContainerName,
                    $"entry/cached/{e.Id.ToString()}-32x32.png");
                if (!exists) {
                    _logger.LogDebug ($"Caching: {e.Id}");
                    var imageUrl = e.ImageUrl.StartsWith ("http") ?
                        e.ImageUrl :
                        $"{_storageSettings.CdnUrl}/{e.ImageUrl}";
                    await CacheImage (
                        imageUrl,
                        e.Id.ToString ());
                } else {
                    _logger.LogDebug ($"Not caching: {e.Id}");
                }
                _logger.LogDebug ($"Processing {i++} of {count}");
            }

            return true;
        }

        public async Task < (string, string) > CacheImage (Guid entryId) {
            var entry = await _entryRepository.GetAsync (entryId);
            if (entry is null) return (string.Empty, string.Empty);

            var (original, thumbnail) = await CacheImage (entry.ImageUrl, entry.Id.ToString ());

            if (string.IsNullOrEmpty (original)) return (string.Empty, string.Empty);

            entry.ImageUrl = original;
            await _unitOfWork.CompleteAsync ();

            return (original, thumbnail);
        }

        public async Task < (string, string) > CacheImage (string imageUrl, string destUid) {
            try {
                var sourceFile = await HttpUtils.DownloadFile (imageUrl);
                var thumbnailFile = ImageUtils.CreateThumbnail (sourceFile, destUid, 32, 32);

                var original = await _fileUploader.UploadFile (
                    sourceFile,
                    _imageFileStoragesSettings.ContainerName,
                    $"entry/{destUid}.png",
                    "image/png", null);
                var thumbnail = await _fileUploader.UploadFile (
                    thumbnailFile,
                    _imageFileStoragesSettings.ContainerName,
                    $"entry/cached/{destUid}-32x32.png",
                    "image/png", null);

                return (original, thumbnail);
            } catch (Exception ex) {
                _logger.LogError ($"Error caching image: {ex.Message}");
            }

            return (string.Empty, string.Empty);
        }
    }
}