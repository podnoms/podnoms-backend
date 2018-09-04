using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Notifications;
using PodNoms.Api.Services.Push;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Services.Jobs {
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
            this._notificationService = notificationService;
            this._subscriptionStore = subscriptionStore;
            this._notificationRepository = notificationRepository;
            this._logger = logger;
            this._handlers = serviceProvider.GetServices<INotificationHandler>().ToArray();
        }
        public async Task NotifyUser(string userId, string title, string body, string image) {
            var pushMessage = new WP.PushMessage(body)
            {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            await _subscriptionStore.ForEachSubscriptionAsync(userId, (WP.PushSubscription subscription) => {
                _notificationService.SendNotificationAsync(subscription, pushMessage);
            });
        }
        public async Task SendCustomNotifications(Guid podcastId, string title, string body) {
            var id = podcastId.ToString();
            _logger.LogDebug($"Finding custom notifications for {id}");
            var notifications = _notificationRepository.GetAll().Where(r => r.PodcastId == podcastId);
            foreach (var notification in notifications) {
                _logger.LogDebug($"Found notification: {notification.Type.ToString()}");
                foreach (BaseNotificationHandler handler in _handlers) {
                    if (notification.Type != handler.Type) continue;
                    
                    _logger.LogDebug($"Found handler: {notification.Type.ToString()}");
                    await handler.SendNotification(notification.Id, title, body);
                }
            }
        }
    }
}
