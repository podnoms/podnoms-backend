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
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class AudioController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioStorageSettings;

        public AudioController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IRepoAccessor repo,
            ILogger<AudioController> logger,
            IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings
        ) : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
        }

        [AllowAnonymous]
        [HttpGet("{entryId}")]
        public async Task<ActionResult<string>> Get(string entryId) {
            var cleanedId = Path.GetFileNameWithoutExtension(entryId);

            var entry = await _repo.Entries.GetAsync(cleanedId);
            if (entry == null) {
                return NotFound();
            }

            var httpRequestFeature = _httpContextAccessor.HttpContext?.Features.Get<IHttpRequestFeature>();
            var target = httpRequestFeature?.RawTarget;

            await _repo.ActivityLogPodcastEntry.AddLogEntry(
                entry,
                _httpContextAccessor.HttpContext?.Request.Headers["Referer"].ToString(),
                _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
                _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString());
            return Redirect(
                $"{_storageSettings.CdnUrl}/{_audioStorageSettings.ContainerName}/{entry.Id}.mp3?ngsw-bypass");
        }
    }
}
