using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("/podcast/{slug}/imageupload")]
    public class ImageUploadController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        public readonly IFileUploader _fileUploader;
        private readonly StorageSettings _storageSettings;

        public ImageUploadController(IPodcastRepository repository, IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IFileUploader fileUploader, IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            ILogger<ImageUploadController> logger, IMapper mapper, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _fileUploader = fileUploader;
            _storageSettings = storageSettings.Value;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _podcastRepository = repository;
            this._entryRepository = entryRepository;
            //this._repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost("/podcast/{id}/imageupload")]
        public async Task<ActionResult<string>> UploadPodcastImage(string id, IFormFile image) {
            _logger.LogDebug("Uploading new image");

            var podcast = await _podcastRepository.GetAsync(_applicationUser.Id, Guid.Parse(id));
            if (podcast is null)
                return NotFound();
            try {
                var imageFile = await _commitImage(id, image, "podcast");
                _podcastRepository.AddOrUpdate(podcast);
                await _unitOfWork.CompleteAsync();

                return Ok($"\"{_mapper.Map<Podcast, PodcastViewModel>(podcast).ImageUrl}\"");
            } catch (InvalidOperationException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/entry/{id}/imageupload")]
        public async Task<ActionResult<PodcastEntryViewModel>> UploadEntryImage(string id, IFormFile image) {
            _logger.LogDebug("Uploading new entry image");

            var entry = await _entryRepository.GetAsync(_applicationUser.Id, id);
            if (entry is null)
                return NotFound();
            try {
                var imageFile = await _commitImage(id, image, "entry");
                entry.ImageUrl = $"{_imageFileStorageSettings.ContainerName}/{imageFile}";
                _entryRepository.AddOrUpdate(entry);
                await _unitOfWork.CompleteAsync();

                return Ok(_mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry));
            } catch (InvalidOperationException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/profile/{id}/imageupload")]
        public async Task<ActionResult<ProfileViewModel>> UploadProfileImage(string id, IFormFile image) {
            var imageFile = await _commitImage(id, image, "profile");
            _applicationUser.PictureUrl = string.Empty; //Image is cached
            await _userManager.UpdateAsync(_applicationUser);
            return Ok(_mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser));
        }

        private async Task<string> _commitImage(string id, IFormFile image, string subDirectory) {
            if (image is null) {
                throw new InvalidOperationException("Image in stream is null");
            }

            if (image is null || image.Length == 0) {
                throw new InvalidOperationException("Image in stream has zero length");
            }

            if (image.Length > _imageFileStorageSettings.MaxUploadFileSize) {
                throw new InvalidOperationException("Maximum file size exceeded");
            }

            if (!_imageFileStorageSettings.IsSupported(image.FileName)) {
                throw new InvalidOperationException("Invalid file type");
            }

            var cacheFile = await CachedFormFileStorage.CacheItem(System.IO.Path.GetTempPath(), image);
            var (finishedFile, extension) = await ImageUtils.ConvertFile(cacheFile, id);
            var destinationFile = $"{subDirectory}/{id}.{extension}";

            await _fileUploader.UploadFile(
                finishedFile,
                _imageFileStorageSettings.ContainerName,
                destinationFile,
                "image/png",
                (p, t) => _logger.LogDebug($"Uploading image: {p} - {t}")
            );
            return destinationFile;
        }
    }
}
