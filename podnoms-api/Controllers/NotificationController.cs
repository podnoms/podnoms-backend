using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Jobs;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class NotificationController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly INotifyJobCompleteService _notifyJobCompleteService;
        private readonly IMapper _mapper;

        public NotificationController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificationController> logger,
            INotifyJobCompleteService notifyJobCompleteService,
            IMapper mapper, IRepoAccessor repo) :
            base(contextAccessor, userManager, logger) {
            _repo = repo;
            _notifyJobCompleteService = notifyJobCompleteService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationViewModel>>> Get(string podcastId) {
            var notifications = await _repo.Notifications
                .GetAll()
                .Where(n => n.PodcastId.ToString() == podcastId)
                .ToListAsync();
            return Ok(_mapper.Map<List<Notification>, List<NotificationViewModel>>(notifications));
        }

        [HttpPost]
        public async Task<ActionResult<NotificationViewModel>> Post([FromBody] NotificationViewModel notification) {
            var model = _mapper.Map<NotificationViewModel, Notification>(notification);
            var ret = await _repo.Notifications.AddOrUpdate(model);
            await _repo.CompleteAsync();
            return Ok(_mapper.Map<Notification, NotificationViewModel>(ret));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id) {
            if (!Guid.TryParse(id, out var parsedId)) return BadRequest("Invalid id");

            await _repo.Notifications.DeleteAsync(parsedId);
            await _repo.CompleteAsync();
            return Ok();
        }

        [HttpGet("logs")]
        public async Task<ActionResult<IList<NotificationLog>>> GetLogs(string notificationId, int take = 10) {
            var logs = await _repo.Notifications.GetLogsAsync(notificationId);
            return logs
                .Take(take)
                .ToList();
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
        public async Task<ActionResult> NotifyUser(string userId, string title, string body, string target,
            string image) {
            var authToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer", string.Empty);

            //think this should be okay? I can't pass this to the jobs server
            //as it doesn't have access the the sqlite registration store
            //TODO: we should probably move all the sqlite registration store
            //TODO: out of the API and into the job server
            await _notifyJobCompleteService.NotifyUser(userId, "PodNoms",
                $"{title} has finished processing",
                target,
                image, NotificationOptions.UploadCompleted);
            return Accepted();
        }

        [HttpPost("sendcustomnotifications")]
        public ActionResult SendCustomNotifications(string podcastId, string title, string body, string url) {
            var authToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer", string.Empty);

            //think this should be okay? I can't pass this to the jobs server
            //as it doesn't have access the the sqlite registration store
            //TODO: we should probably move all the sqlite registration store
            //TODO: out of the API and into the job server
            _notifyJobCompleteService.SendCustomNotifications(
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
