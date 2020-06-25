using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Hubs;
using PodNoms.Common.Services.Middleware;
using PodNoms.Common.Services.Push;
using WP = Lib.Net.Http.WebPush;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Auth;

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
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly HubLifetimeManager<DebugHub> _hub;
        private readonly IConfiguration _config;
        private readonly IPageParser _pageParser;
        private readonly IMapper _mapper;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IEntryRepository _entryRepository;
        private readonly IPushNotificationService _notificationService;
        private readonly IPodcastRepository _podcastRepository;
        private readonly IUnitOfWork _unitOfWork;
        public readonly AppSettings _appSettings;
        private readonly IMailSender _mailSender;

        public DebugController(IOptions<StorageSettings> settings, IOptions<AppSettings> appSettings,
            HubLifetimeManager<DebugHub> hub,
            IConfiguration config,
            IPageParser pageParser,
            IOptions<HelpersSettings> helpersSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<JwtIssuerOptions> jwtIssuerOptions,
            IPushSubscriptionStore subscriptionStore,
            IEntryRepository entryRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<DebugController> logger,
            IMapper mapper,
            IPushNotificationService notificationService,
            IPodcastRepository podcastRepository,
            IUnitOfWork unitOfWork,
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
            _pageParser = pageParser;
            _mapper = mapper;
            _subscriptionStore = subscriptionStore;
            _entryRepository = entryRepository;
            _notificationService = notificationService;
            _podcastRepository = podcastRepository;
            _unitOfWork = unitOfWork;
            _mailSender = mailSender;
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

        [HttpGet("generatelogdata")]
        public IActionResult GenerateLogData() {
            for (var i = 0; i < 1000; i++) {
                _logger.LogError($"Debug error {i}");
            }

            return Ok();
        }

        [HttpGet("updateentryslugs")]
        public async Task<IActionResult> UpdateEntrySlugs() {
            var entries = _entryRepository.GetAll();
            foreach (var entry in entries) {
                entry.UpdateDate = System.DateTime.Today;
            }

            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("sendmail")]
        public async Task<IActionResult> SendEmail(string email) {
            await _mailSender.SendEmailAsync(
                email,
                "Debug Message",
                new MailDropin {
                    username = "Handsome Fucker",
                    message = "Hello Sailor"
                });
            return Ok();
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
            var pushMessage = new WP.PushMessage(message) {
                Topic = "Debug",
                Urgency = WP.PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(_applicationUser.Id, (subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage, "http://fergl.ie");
                response.Append($"Sent: {subscription.Endpoint}");
            });
            return response.ToString();
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

        [HttpGet("longrunningrequest")]
        public async Task<IActionResult> LongRunningRequest([FromQuery] int delay) {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("readogtags")]
        public async Task<IActionResult> ReadOgTags([FromQuery] string url) {
            // var parser = await _pageParser.Create(url);
            if (await _pageParser.Initialise(url)) {
                var title = _pageParser.GetHeadTag("og:title");
                var image = _pageParser.GetHeadTag("og:image");
                var description = _pageParser.GetHeadTag("og:description");
                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(image)) {
                    return Ok(new {
                        title = title,
                        description = description,
                        image = image
                    });
                }
            }

            return BadRequest("Invalid Url");
        }
    }
}
