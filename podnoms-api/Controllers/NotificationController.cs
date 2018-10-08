using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;
using PodNoms.Common;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class NotificationController : BaseAuthController {
        private readonly ISupportChatService _supportChatService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<NotificationController> logger,
            IMapper mapper, IUnitOfWork unitOfWork, INotificationRepository notificationRepository,
            ISupportChatService supportChatService) :
            base(contextAccessor, userManager, logger) {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
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
    }
}