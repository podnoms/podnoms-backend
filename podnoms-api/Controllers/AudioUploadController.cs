using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PodNoms.Data.Models;
using PodNoms.Api.Providers;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Storage;
using PodNoms.Data.Enums;
using PodNoms.Common.Services.Jobs;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("/podcast/{slug}/audioupload")]
    public class AudioUploadController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _entryRepository;
        private IUnitOfWork _unitOfWork;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        public IMapper _mapper { get; }

        public AudioUploadController(IPodcastRepository podcastRepository, IEntryRepository entryRepository, IUnitOfWork unitOfWork,
                        IOptions<AudioFileStorageSettings> settings, IOptions<StorageSettings> storageSettings,
                        ILogger<AudioUploadController> logger, IMapper mapper,
                        UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _mapper = mapper;
            _audioFileStorageSettings = settings.Value;
            _storageSettings = storageSettings.Value;
            _podcastRepository = podcastRepository;
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string slug, IFormFile file) {
            _logger.LogDebug($"Settings are\nMaxUploadFileSize: {_audioFileStorageSettings.MaxUploadFileSize}");
            if (file is null || file.Length == 0) return BadRequest("No file found in stream");
            if (file.Length > _audioFileStorageSettings.MaxUploadFileSize) return BadRequest("Maximum file size exceeded");
            if (!_audioFileStorageSettings.IsSupported(file.FileName)) return BadRequest("Invalid file type");

            var podcast = await _podcastRepository.GetForUserAndSlugAsync(_applicationUser.Slug, slug);
            if (podcast is null)
                return NotFound();

            var entry = new PodcastEntry {
                Title = Path.GetFileName(Path.GetFileNameWithoutExtension(file.FileName)),
                ImageUrl = $"{_storageSettings.CdnUrl}static/images/default-entry.png",
                Processed = false,
                ProcessingStatus = ProcessingStatus.Processing,
                Podcast = podcast
            };

            var localFile = await CachedFormFileStorage.CacheItem(file);
            _entryRepository.AddOrUpdate(entry);
            await _unitOfWork.CompleteAsync();

            var authToken = _httpContext.Request.Headers["Authorization"].ToString();
            BackgroundJob.Enqueue<UploadAudioJob>(job =>
                job.Execute(authToken, entry.Id, localFile, null));

            var ret = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return Ok(ret);
        }
    }
}
