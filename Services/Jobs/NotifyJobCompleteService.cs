using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using PodNoms.Api.Services.Push;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Services.Jobs {
    public class NotifyJobCompleteService : INotifyJobCompleteService {
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;

        public NotifyJobCompleteService(IPushSubscriptionStore subscriptionStore,
                IPushNotificationService notificationService) {
            this._notificationService = notificationService;
            this._subscriptionStore = subscriptionStore;
        }
        public async Task NotifyUser(string userId, string title, string body, string image) {
            WP.PushMessage pushMessage = new WP.PushMessage(body) {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(userId, (WP.PushSubscription subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage);
            });
        }
    }
}
