using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Api.Services.Push;
using PodNoms.Api.Services.Push.Models;
using PodNoms.Api.Persistence;
using Microsoft.AspNetCore.Identity;
using PodNoms.Api.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Controllers {

    // [Authorize]
    [Route("[controller]")]
    public class WebPushController : BaseAuthController {
        private readonly IPushSubscriptionStore _subscriptionStore;
        public readonly IPushNotificationService _notificationService;

        public WebPushController(IPushSubscriptionStore subscriptionStore, IPushNotificationService notificationService,
                                    UserManager<ApplicationUser> userManager, ILogger<WebPushController> logger,
                                    IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            this._subscriptionStore = subscriptionStore;
            this._notificationService = notificationService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> StoreSubscription([FromBody]WP.PushSubscription subscription) {
            await _subscriptionStore.StoreSubscriptionAsync(_applicationUser.Id, subscription);
            return NoContent();
        }

        // POST push-notifications-api/notifications
        [HttpPost("message")]
        public async Task<IActionResult> SendNotification([FromBody]PushMessageViewModel message) {
            WP.PushMessage pushMessage = new WP.PushMessage(message.Notification) {
                Topic = message.Topic,
                Urgency = message.Urgency
            };

            // TODO: This should be scheduled in background
            await _subscriptionStore.ForEachSubscriptionAsync((WP.PushSubscription subscription) => {
                // Fire-and-forget 
                _notificationService.SendNotificationAsync(subscription, pushMessage);
            });

            return NoContent();
        }
    }
}
