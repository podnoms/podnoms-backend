using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Settings;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Downloader;
using PodNoms.Api.Services.Hubs;
using PodNoms.Api.Services.Jobs;
using PodNoms.Api.Services.Push;
using PodNoms.Api.Services.Realtime;
using WebPush = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Controllers {
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    public class DebugController : BaseAuthController {
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly HelpersSettings _helpersSettings;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly HubLifetimeManager<DebugHub> _hub;
        private readonly IConfiguration _config;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;

        public readonly AppSettings _appSettings;

        public DebugController(IOptions<StorageSettings> settings, IOptions<AppSettings> appSettings,
            HubLifetimeManager<DebugHub> hub,
            IConfiguration config,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<JwtIssuerOptions> jwtIssuerOptions,
            IPushSubscriptionStore subscriptionStore,
            UserManager<ApplicationUser> userManager,
            IPushNotificationService notificationService,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager) {
            this._appSettings = appSettings.Value;
            this._storageSettings = settings.Value;
            this._helpersSettings = helpersSettings.Value;
            this._audioFileStorageSettings = audioFileStorageSettings.Value;
            this._imageFileStorageSettings = imageFileStorageSettings.Value;
            this._jwtIssuerOptions = jwtIssuerOptions.Value;
            this._hub = hub;
            this._config = config;
            this._subscriptionStore = subscriptionStore;
            this._notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get() {
            var config = new {
                Version = _appSettings.Version,
                CdnUrl = _storageSettings.CdnUrl,
                AudioContainer = _audioFileStorageSettings.ContainerName,
                ImageContainer = _imageFileStorageSettings.ContainerName,
                YouTubeDlPath = _helpersSettings.Downloader,
                YouTubeDlVersion = AudioDownloader.GetVersion(_helpersSettings.Downloader),
                OSVersion = System.Environment.OSVersion,
                RssUrl = _appSettings.RssUrl
            };
            return new OkObjectResult(config);
        }
        [AllowAnonymous]
        [HttpGet("getoptions")]
        public IActionResult GetOptions() {
            var response = new {
                AppSettings = _config.GetSection("App").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                StorageSettings = _config.GetSection("Storage").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                HelpersSettings = _config.GetSection("HelpersSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                EmailSettings = _config.GetSection("EmailSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                FacebookAuthSettings = _config.GetSection("FacebookAuthSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                ChatSettings = _config.GetSection("ChatSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                ImageFileStorageSettings = _config.GetSection("ImageFileStorageSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                AudioFileStorageSettings = _config.GetSection("AudioFileStorageSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                JwtIssuerOptions = _config.GetSection("JwtIssuerOptions").GetChildren().Select(c => new { Key = c.Key, Value = c.Value })
            };
            return Ok(JsonConvert.SerializeObject(response));
        }
        [Authorize]
        [HttpPost("realtime")]
        public async Task<IActionResult> Realtime([FromBody] string message) {
            await _hub.SendUserAsync(User.Identity.Name, "Send", new string[] { $"User {User.Identity.Name}: {message}" });
            await _hub.SendAllAsync("Send", new string[] { $"All: {message}" });
            return Ok(message);
        }
        [Authorize]
        [HttpGet("serverpush")]
        public async Task<string> ServerPush() {
            WebPush.PushMessage pushMessage = new WebPush.PushMessage("Argle Bargle, Foo Ferra") {
                Topic = "Debug",
                Urgency = WebPush.PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(_applicationUser.Id, (subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage);
            });
            return "Hello Sailor!";
        }
    }
}
