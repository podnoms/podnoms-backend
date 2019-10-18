using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Apis.Logging;
using Google.Apis.YouTube.v3.Data;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PodNoms.Common;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Jobs;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class NotificationController : BaseAuthController {
        private readonly ISupportChatService _supportChatService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;
        private readonly INotifyJobCompleteService _jobCompleteNotificationService;
        private readonly IMapper _mapper;

        public NotificationController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                ILogger<NotificationController> logger,
                INotifyJobCompleteService jobCompleteNotificationService,
                IOptions<AppSettings> appSettings,
                IMapper mapper, IUnitOfWork unitOfWork, INotificationRepository notificationRepository,
                ISupportChatService supportChatService) :
            base(contextAccessor, userManager, logger) {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _appSettings = appSettings.Value;
            _jobCompleteNotificationService = jobCompleteNotificationService;
            _mapper = mapper;
            _supportChatService = supportChatService;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationViewModel>>> Get(string podcastId) {
            var notifications = await _notificationRepository
                .GetAll()
                .Where(n => n.PodcastId.ToString() == podcastId)
                .ToListAsync();
            return Ok(_mapper.Map<List<Notification>, List<NotificationViewModel>>(notifications));
        }

        [HttpPost]
        public async Task<ActionResult<NotificationViewModel>> Post([FromBody] NotificationViewModel notification) {
            var model = _mapper.Map<NotificationViewModel, Notification>(notification);
            var ret = _notificationRepository.AddOrUpdate(model);
            await _unitOfWork.CompleteAsync();
            return Ok(_mapper.Map<Notification, NotificationViewModel>(ret));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id) {
            if (!Guid.TryParse(id, out var parsedId)) return BadRequest("Invalid id");

            await _notificationRepository.DeleteAsync(parsedId);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpGet("logs")]
        public async Task<ActionResult<IList<NotificationLog>>> GetLogs(string notificationId) {
            var logs = await _notificationRepository.GetLogsAsync(notificationId);
            return logs.ToList();
        }

        [HttpGet("types")]
        public ActionResult<IList<string>> GetTypes(string type) {
            var types = Enum.GetValues(typeof(Notification.NotificationType))
                .Cast<Notification.NotificationType>()
                .Select(t => t.ToString())
                .ToList();
            return Ok(types);
        }

        [HttpGet("config")]
        public ActionResult<NotificationConfigViewModel> GetConfig(string type) {
            var config = BaseNotificationConfig.GetConfig(type);
            if (config != null)
                return Ok(_mapper.Map<BaseNotificationConfig, NotificationConfigViewModel>(config));

            return NotFound();
        }

        [HttpPost("notifyuser")]
        public async Task<ActionResult> NotifyUser(string userId, string title, string body, string target, string image) {
            var authToken = _httpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer", string.Empty);

            //think this should be okay? I can't pass this to the jobs server
            //as it doesn't have access the the sqlite registration store
            //TODO: we should probably move all the sqlite registration store
            //TODO: out of the API and into the job server
            await _jobCompleteNotificationService.NotifyUser(userId, "PodNoms",
                $"{title} has finished processing",
                target,
                image, NotificationOptions.UploadCompleted);
            return Accepted();
        }
        [HttpPost("sendcustomnotifications")]
        public ActionResult SendCustomNotifications(string podcastId, string title, string body, string url) {
            var authToken = _httpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer", string.Empty);

            //think this should be okay? I can't pass this to the jobs server
            //as it doesn't have access the the sqlite registration store
            //TODO: we should probably move all the sqlite registration store
            //TODO: out of the API and into the job server
            _jobCompleteNotificationService.SendCustomNotifications(
                Guid.Parse(podcastId),
                _applicationUser.GetBestGuessName(),
                title,
                body,
                url
            );
            return Accepted();
        }
    }
}
