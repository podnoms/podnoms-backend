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
using PodNoms.Api.Models;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class NotificationController : BaseAuthController {
        private readonly ISupportChatService _supportChatService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager, ILogger<ChatController> logger,
                            IMapper mapper, IUnitOfWork unitOfWork, INotificationRepository notificationRepository, ISupportChatService supportChatService) :
            base(contextAccessor, userManager, logger) {
            this._notificationRepository = notificationRepository;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._supportChatService = supportChatService;
        }
        [HttpGet]
        public async Task<ActionResult<List<NotificationViewModel>>> Get(string podcastId, string type) {
            if (Enum.TryParse(type, out NotificationType parsedType)) {
                var notifications = await _notificationRepository
                    .GetAll()
                    .Where(n => n.PodcastId.ToString() == podcastId)
                    .Where(n => n.Type.Equals(parsedType))
                    .ToListAsync();
                return Ok(_mapper.Map<List<Notification>, List<NotificationViewModel>>(notifications));
            } else {
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<ActionResult<NotificationViewModel>> Post([FromBody]NotificationViewModel notification) {
            var model = _mapper.Map<NotificationViewModel, Notification>(notification);
            var ret = _notificationRepository.AddOrUpdate(model);
            await _unitOfWork.CompleteAsync();
            return Ok(_mapper.Map<Notification, NotificationViewModel>(ret));
        }
        [HttpGet("types")]
        public ActionResult<IList<string>> GetTypes(string type) {
            var types = Enum.GetValues(typeof(NotificationType))
                .Cast<NotificationType>()
                .Select(t => t.ToString())
                .ToList();
            return Ok(types);
        }
        [HttpGet("config")]
        public ActionResult<NotificationConfigViewModel> GetConfig(/*string podcastId, */string type) {
            var slackNotificationConfig = new SlackNotificationConfig();
            var config = _mapper.Map<BaseNotificationConfig, NotificationConfigViewModel>(slackNotificationConfig);
            return Ok(config);
        }
    }
}
