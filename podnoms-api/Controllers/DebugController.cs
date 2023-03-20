using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Services.Downloader;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Controllers {
}

namespace PodNoms.Api.Controllers {
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    public class DebugController : BaseAuthController {
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly HelpersSettings _helpersSettings;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        private readonly IConfiguration _config;
        private readonly AppSettings _appSettings;

        public DebugController(IOptions<StorageSettings> settings,
            IOptions<AppSettings> appSettings,
            IConfiguration config,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            UserManager<ApplicationUser> userManager,
            ILogger<DebugController> logger,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _appSettings = appSettings.Value;
            _storageSettings = settings.Value;
            _helpersSettings = helpersSettings.Value;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _config = config;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
        public IActionResult Get() {
            var config = new {
                Version = _config["Version"],
                CdnUrl = _storageSettings.CdnUrl,
                AudioContainer = _audioFileStorageSettings.ContainerName,
                ImageContainer = _imageFileStorageSettings.ContainerName,
                YouTubeDlPath = _helpersSettings.Downloader,
                YouTubeDlVersion = AudioDownloader.GetVersion(_helpersSettings.Downloader),
                OSVersion = Environment.OSVersion,
                RssUrl = _appSettings.RssUrl
            };
            return Ok(config);
        }
    }
}
