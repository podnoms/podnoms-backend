using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;

namespace PodNoms.Common.Services.Processor {
    public class RemoteImageCacher {
        private readonly ILogger<RemoteImageCacher> _logger;
        private readonly IFileUploader _fileUploader;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;

        public RemoteImageCacher(ILogger<RemoteImageCacher> logger, IFileUploader fileUploader,
            IOptions<ImageFileStorageSettings> imageFileStoragesSettings
        ) {
            _logger = logger;
            _fileUploader = fileUploader;
            _imageFileStorageSettings = imageFileStoragesSettings.Value;
        }

        public async Task<string> CacheImage(string imageUrl, string destUid) {
            // TODO: Need to convert everything to jpeg
            // PNG was a bad choice
            try {
                var sourceFile = await HttpUtils.DownloadFile(imageUrl);
                if (string.IsNullOrEmpty(sourceFile))
                    return string.Empty;
                var extension = await HttpUtils.GetUrlExtension(imageUrl);

                if (!extension.Equals("jpg")) {
                    (sourceFile, extension) = ImageUtils.ConvertFile(sourceFile, sourceFile, "jpg");
                }

                var remoteFile = await _fileUploader.UploadFile(
                    sourceFile,
                    _imageFileStorageSettings.ContainerName,
                    $"entry/{destUid}.jpg",
                    "image/jpeg");

                return remoteFile;
            } catch (Exception ex) {
                _logger.LogError($"Error caching image: {ex.Message}");
            }

            return string.Empty;
        }
    }
}
