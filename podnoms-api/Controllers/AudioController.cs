using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class AudioController : BaseAuthController {
        private readonly IEntryRepository _entryRepository;
        private readonly IActivityLogPodcastEntryRepository _activityRepository;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioStorageSettings;

        public AudioController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<AudioController> logger,
            IEntryRepository entryRepository,
            IOptions<AppSettings> appSettings,
            IOptions<StorageSettings> storageSettings,
            IActivityLogPodcastEntryRepository activityRepository,
            IOptions<AudioFileStorageSettings> audioStorageSettings
            ) : base(contextAccessor, userManager, logger) {
            _entryRepository = entryRepository;
            _activityRepository = activityRepository;
            _appSettings = appSettings.Value;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
        }
        [AllowAnonymous]
        [HttpGet("{entryId}")]
        public async Task<ActionResult<string>> Get(string entryId) {

            var cleanedId = Path.GetFileNameWithoutExtension(entryId);

            var entry = await _entryRepository.GetAsync(cleanedId);
            if (entry != null) {

                var httpRequestFeature = _httpContextAccessor.HttpContext.Features.Get<IHttpRequestFeature>();
                var target = httpRequestFeature.RawTarget;
                await _activityRepository.AddLogEntry(
                    entry,
                    _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(),
                    _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
                    _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
                return Redirect(
                    $"{_storageSettings.CdnUrl}/{_audioStorageSettings.ContainerName}/{entry.Id}.mp3?ngsw-bypass");
            }

            return NotFound();
        }
    }
}
