using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Push;
using PodNoms.Common.Services.Push.Models;
using PodNoms.Data.Models;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Controllers {

    // [Authorize]
    [Route("[controller]")]
    public class WebPushController : BaseAuthController {
        private readonly IPushSubscriptionStore _subscriptionStore;
        public readonly IPushNotificationService _notificationService;

        public WebPushController(
                IPushSubscriptionStore subscriptionStore, IPushNotificationService notificationService,
                UserManager<ApplicationUser> userManager, ILogger<WebPushController> logger,
                IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _subscriptionStore = subscriptionStore;
            _notificationService = notificationService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> StoreSubscription([FromBody]WP.PushSubscription subscription) {
            var subscriptionId = await _subscriptionStore.StoreSubscriptionAsync(_applicationUser.Id, subscription);
            _logger.LogDebug($"Creating push registration for {_applicationUser.Id} with subscriptionId of {subscriptionId}");
            return Ok(new {
                uid = subscriptionId,
                status = true
            });
        }

        // POST push-notifications-api/notifications
        [HttpPost("message")]
        public async Task<IActionResult> SendNotification([FromBody]PushMessageViewModel message) {
            _logger.LogInformation($"Sending targeted push for: {message.Target} - {message.Notification}");
            var pushMessage = new WP.PushMessage(message.Notification) {
                Topic = message.Topic,
                Urgency = message.Urgency
            };

            // TODO: This should be scheduled in background
            await _subscriptionStore.ForEachSubscriptionAsync(message.Target, (WP.PushSubscription subscription) => {
                _logger.LogInformation($"Found subscription: {subscription}");
                _notificationService.SendNotificationAsync(subscription, pushMessage, message.Target);
            });

            return NoContent();
        }
    }
}
