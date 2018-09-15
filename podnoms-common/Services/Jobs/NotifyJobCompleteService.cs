using System;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Notifications;
using PodNoms.Common.Services.Push;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Jobs {
    public class NotifyJobCompleteService : INotifyJobCompleteService {
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;

        private readonly ILogger<NotifyJobCompleteService> _logger;
        private readonly INotificationHandler[] _handlers;

        public NotifyJobCompleteService(IPushSubscriptionStore subscriptionStore,
            IPushNotificationService notificationService,
            INotificationRepository notificationRepository,
            ILogger<NotifyJobCompleteService> logger,
            IServiceProvider serviceProvider) {
            _notificationService = notificationService;
            _subscriptionStore = subscriptionStore;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _handlers = serviceProvider.GetServices<INotificationHandler>().ToArray();
        }

        public async Task NotifyUser(string userId, string title, string body, string image) {
            var pushMessage = new PushMessage(body) {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(userId,
                subscription => {
                    _notificationService.SendNotificationAsync(subscription, pushMessage);
                });
        }

        public async Task SendCustomNotifications(Guid podcastId, string title, string body) {
            var id = podcastId.ToString();
            _logger.LogDebug($"Finding custom notifications for {id}");
            var notifications = _notificationRepository.GetAll().Where(r => r.PodcastId == podcastId);
            foreach (var notification in notifications) {
                _logger.LogDebug($"Found notification: {notification.Type.ToString()}");
                var typeHandlers = _handlers.Where(h => h.Type == notification.Type);
                foreach (var handler in typeHandlers) {
                    _logger.LogDebug($"Found handler: {notification.Type.ToString()}");
                    await handler.SendNotification(notification.Id, title, body);
                }
            }
        }
    }
}