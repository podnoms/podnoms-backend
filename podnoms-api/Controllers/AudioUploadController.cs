using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Storage;
using PodNoms.Data.Enums;
using PodNoms.Common.Services.Jobs;
using Microsoft.AspNetCore.Hosting;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("/podcast/{slug}/audioupload")]
    public class AudioUploadController : BaseAuthController {
        private IRepoAccessor _repo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly IMapper _mapper;

        public AudioUploadController(
            IRepoAccessor repo,
            IOptions<AudioFileStorageSettings> settings,
            IOptions<StorageSettings> storageSettings,
            ILogger<AudioUploadController> logger,
            IMapper mapper,
            IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _mapper = mapper;
            _audioFileStorageSettings = settings.Value;
            _storageSettings = storageSettings.Value;
            _repo = repo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string slug, IFormFile file) {
            if (file is null || file.Length == 0) return BadRequest("No file found in stream");
            if (file.Length > _audioFileStorageSettings.MaxUploadFileSize)
                return BadRequest("Maximum file size exceeded");
            if (!_audioFileStorageSettings.IsSupported(file.FileName)) return BadRequest("Invalid file type");

            var podcast = await _repo.Podcasts.GetForUserAndSlugAsync(_applicationUser.Slug, slug);
            if (podcast is null) {
                _logger.LogError($"Unable to find podcast");
                return NotFound();
            }

            var entry = new PodcastEntry {
                Title = Path.GetFileName(Path.GetFileNameWithoutExtension(file.FileName)),
                ImageUrl = $"{_storageSettings.CdnUrl}/static/images/default-entry.png",
                Processed = false,
                ProcessingStatus = ProcessingStatus.Processing,
                Podcast = podcast 
            };

            var localFile = await CachedFormFileStorage.CacheItem(_hostingEnvironment.WebRootPath, file);
            _logger.LogDebug($"Local file is: {localFile}");

            _repo.Entries.AddOrUpdate(entry);

            _logger.LogDebug("Completing uow");
            await _repo.CompleteAsync();

            var authToken = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authToken)) {
                return Unauthorized("Auth token is empty");
            }

            //convert uploaded file to extension
            var audioUrl = localFile
                .Replace(_hostingEnvironment.WebRootPath, string.Empty)
                .Replace(@"\", "/");

            _logger.LogDebug($"Starting processing jobs for url: {audioUrl}");

            BackgroundJob.Enqueue<ProcessNewEntryJob>(e =>
                e.ProcessEntryFromUploadFile(entry.Id, audioUrl, authToken, null));

            var ret = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return Ok(ret);
        }
    }
}
