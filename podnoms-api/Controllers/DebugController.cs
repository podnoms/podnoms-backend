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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Data.Models;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Hubs;
using PodNoms.Common.Services.Middleware;
using PodNoms.Common.Services.Push;
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
        private readonly IMailSender _mailSender;

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
            IHttpContextAccessor contextAccessor,
            IMailSender mailSender) : base(contextAccessor, userManager, logger) {
            _appSettings = appSettings.Value;
            _storageSettings = settings.Value;
            _helpersSettings = helpersSettings.Value;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _jwtIssuerOptions = jwtIssuerOptions.Value;
            _hub = hub;
            _config = config;
            _mapper = mapper;
            _subscriptionStore = subscriptionStore;
            _notificationService = notificationService;
            _podcastRepository = podcastRepository;
            _mailSender = mailSender;
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
                OSVersion = Environment.OSVersion,
                RssUrl = _appSettings.RssUrl
            };
            return Ok(config);
        }

        [AllowAnonymous]
        [HttpGet("generatelogdata")]
        public IActionResult GenerateLogData() {
            for (var i = 0; i < 1000; i++) {
                _logger.LogError($"Debug error {i}");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("sendmail")]
        public async Task<IActionResult> SendEmail() {
            await _mailSender.SendEmailAsync("fergal.moran+podnoms@gmail.com", "Debug Message", "Hello Sailor");
            return Ok();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("getoptions")]
        public IActionResult GetOptions() {
            var response = new {
                AppSettings = _config.GetSection("AppSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                StorageSettings = _config.GetSection("StorageSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                HelpersSettings = _config.GetSection("HelpersSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                EmailSettings = _config.GetSection("EmailSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                FacebookAuthSettings = _config.GetSection("FacebookAuthSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                ChatSettings = _config.GetSection("ChatSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                ImageFileStorageSettings = _config.GetSection("ImageFileStorageSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                AudioFileStorageSettings = _config.GetSection("AudioFileStorageSettings").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value }),
                JwtIssuerOptions = _config.GetSection("JwtIssuerOptions").GetChildren()
                    .Select(c => new { Key = c.Key, Value = c.Value })
            };
            return Ok(JsonConvert.SerializeObject(response));
        }

        [Authorize]
        [HttpPost("realtime")]
        public async Task<IActionResult> Realtime([FromBody] string message) {
            await _hub.SendUserAsync(User.Identity.Name, "Send",
                new string[] { $"User {User.Identity.Name}: {message}" });
            await _hub.SendAllAsync("Send", new string[] { $"All: {message}" });
            return Ok(message);
        }

        [Authorize]
        [HttpGet("serverpush")]
        public async Task<string> ServerPush(string message) {
            var response = new StringBuilder();
            var pushMessage = new WP.PushMessage(message)
            {
                Topic = "Debug",
                Urgency = WP.PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(_applicationUser.Id, (subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage, "http://fergl.ie");
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
            var podcast = await _podcastRepository.GetAll()
                .Where(p => p.Id == Guid.Parse("54f5ea27-9dff-41cf-9944-08d600a180a2"))
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            var response = _mapper.Map<Podcast, PodcastViewModel>(podcast);
            return response;
        }
    }
}