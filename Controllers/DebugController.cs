using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Settings;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Downloader;
using PodNoms.Api.Services.Hubs;
using PodNoms.Api.Services.Jobs;
using PodNoms.Api.Services.Middleware;
using PodNoms.Api.Services.Push;
using PodNoms.Api.Services.Realtime;
using WP = Lib.Net.Http.WebPush;

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
        private readonly IMapper _mapper;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;
        private readonly IPodcastRepository _podcastRepository;
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
            ILogger<DebugController> logger,
            IMapper mapper,
            IPushNotificationService notificationService,
            IPodcastRepository podcastRepository,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            this._appSettings = appSettings.Value;
            this._storageSettings = settings.Value;
            this._helpersSettings = helpersSettings.Value;
            this._audioFileStorageSettings = audioFileStorageSettings.Value;
            this._imageFileStorageSettings = imageFileStorageSettings.Value;
            this._jwtIssuerOptions = jwtIssuerOptions.Value;
            this._hub = hub;
            this._config = config;
            this._mapper = mapper;
            this._subscriptionStore = subscriptionStore;
            this._notificationService = notificationService;
            this._podcastRepository = podcastRepository;
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
            return Ok(config);
        }
        [AllowAnonymous]
        [HttpGet("generatelogdata")]
        public IActionResult GenerateLogData() {
            for (int i = 0; i < 1000; i++) {
                _logger.LogError($"Debug error {i}");
            }
            return Ok();
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("getoptions")]
        public IActionResult GetOptions() {
            var response = new {
                AppSettings = _config.GetSection("AppSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
                StorageSettings = _config.GetSection("StorageSettings").GetChildren().Select(c => new { Key = c.Key, Value = c.Value }),
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
        public async Task<string> ServerPush(string message) {
            var response = new StringBuilder();
            WP.PushMessage pushMessage = new WP.PushMessage(message)
            {
                Topic = "Debug",
                Urgency = WP.PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(_applicationUser.Id, (subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage);
                response.Append($"Sent: {subscription.Endpoint}");
            });
            return response.ToString();
        }

        [AllowAnonymous]
        [HttpGet("exception")]
        public void ThrowException(string text) {
            throw new HttpStatusCodeException(500, text);
        }
        [HttpGet("qry")]
        public async Task<ActionResult<PodcastViewModel>> Query() {
            var podcast = await this._podcastRepository.GetAll()
                .Where(p => p.Id == Guid.Parse("54f5ea27-9dff-41cf-9944-08d600a180a2"))
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            var response = _mapper.Map<Podcast, PodcastViewModel>(podcast);
            return response;
        }
    }
}
