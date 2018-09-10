using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Data.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services;
using PodNoms.Api.Services.Storage;
using PodNoms.Api.Utils;
using Microsoft.AspNetCore.Identity;
using PodNoms.Api.Services.Auth;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using PodNoms.Data.Models.Settings;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("/podcast/{slug}/imageupload")]
    public class ImageUploadController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        public readonly IFileUploader _fileUploader;
        private readonly StorageSettings _storageSettings;

        public ImageUploadController(IPodcastRepository repository, IUnitOfWork unitOfWork,
                IFileUploader fileUploader, IOptions<StorageSettings> storageSettings,
                 IOptions<ImageFileStorageSettings> imageFileStorageSettings,
                ILogger<ImageUploadController> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
            : base(contextAccessor, userManager, logger) {

            this._fileUploader = fileUploader;
            this._storageSettings = storageSettings.Value;
            this._imageFileStorageSettings = imageFileStorageSettings.Value;
            this._podcastRepository = repository;
            //this._repository = repository;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        [HttpPost("/podcast/{id}/imageupload")]
        public async Task<ActionResult<string>> UploadPodcastImage(string id, IFormFile image) {
            _logger.LogDebug("Uploading new image");

            var podcast = await _podcastRepository.GetAsync(_applicationUser.Id, Guid.Parse(id));
            if (podcast == null)
                return NotFound();
            try {
                var result = await _commitImage(id, image, "podcast");
                _podcastRepository.AddOrUpdate(podcast);
                await this._unitOfWork.CompleteAsync();

                return Ok($"\"{_mapper.Map<Podcast, PodcastViewModel>(podcast).ImageUrl}\"");
            } catch (InvalidOperationException ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("/profile/{id}/imageupload")]
        public async Task<ActionResult<string>> UploadProfileImage(string id, IFormFile image) {
            var imageUrl = await _commitImage(id, image, "profile");
            _applicationUser.PictureUrl = $"{_storageSettings.CdnUrl}{_imageFileStorageSettings.ContainerName}/{imageUrl}";
            await _userManager.UpdateAsync(_applicationUser);
            return Ok($"\"{_applicationUser.PictureUrl}\"");
        }
        private async Task<string> _commitImage(string id, IFormFile image, string subDirectory) {

            if (image == null || image.Length == 0) throw new InvalidOperationException("No file found in stream");
            if (image.Length > _imageFileStorageSettings.MaxUploadFileSize) throw new InvalidOperationException("Maximum file size exceeded");
            if (!_imageFileStorageSettings.IsSupported(image.FileName)) throw new InvalidOperationException("Invalid file type");

            var cacheFile = await CachedFormFileStorage.CacheItem(image);
            (var finishedFile, var extension) = ImageUtils.ConvertFile(cacheFile, id);
            var thumbnailFile = ImageUtils.CreateThumbnail(cacheFile, id, 32, 32);

            var destinationFile = $"{subDirectory}/{id}.{extension}";
            var destinationFileThumbnail = $"{subDirectory}/{id}-32x32.{extension}";

            await _fileUploader.UploadFile(finishedFile, _imageFileStorageSettings.ContainerName,
                destinationFile, "image/png", (p, t) => _logger.LogDebug($"Uploading image: {p} - {t}"));

            await _fileUploader.UploadFile(thumbnailFile, _imageFileStorageSettings.ContainerName,
                           destinationFileThumbnail, "image/png", (p, t) => _logger.LogDebug($"Uploading image: {p} - {t}"));
            return destinationFile;
        }
    }
}
