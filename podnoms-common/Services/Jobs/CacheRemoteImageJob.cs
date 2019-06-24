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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
                .GetAll ()
                .Where (r => r.Processed == true)
                .Where (r => r.ImageUrl.StartsWith ("https://i.ytimg.com/"));

            int i = 1;
            int count = images.Count ();
            foreach (var e in images) {
                _logger.LogDebug ($"Caching image for: {e.Id}");
                _logger.LogDebug ($"Caching: {e.Id}");
                await CacheImage (e.Id);
                _logger.LogDebug ($"Processing {i++} of {count}");
            }

            return true;
        }

        public async Task<string> CacheImage (Guid entryId) {
            var entry = await _entryRepository.GetAsync (entryId);
            if (entry is null) return string.Empty;

            var file = await CacheImage (entry.ImageUrl, entry.Id.ToString ());

            if (string.IsNullOrEmpty (file)) return string.Empty;

            entry.ImageUrl = file;
            await _unitOfWork.CompleteAsync ();

            return file;
        }

        public async Task<string> CacheImage (string imageUrl, string destUid) {
            // TODO: Need to convert everything to jpeg
            // PNG was a bad choice
            try {
                var sourceFile = await HttpUtils.DownloadFile (imageUrl);
                if (string.IsNullOrEmpty (sourceFile))
                    return string.Empty;
                var extension = await HttpUtils.GetUrlExtension (imageUrl);

                if (!extension.Equals ("jpg")) {
                    (sourceFile, extension) = ImageUtils.ConvertFile (sourceFile, sourceFile, "jpg");
                }

                var remoteFile = await _fileUploader.UploadFile (
                    sourceFile,
                    _imageFileStoragesSettings.ContainerName,
                    $"entry/{destUid}.jpg",
                    "image/jpeg", null);

                return remoteFile;
            } catch (Exception ex) {
                _logger.LogError ($"Error caching image: {ex.Message}");
            }

            return string.Empty;
        }
    }
}
