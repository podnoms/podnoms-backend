using System;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHandler[] _handlers;

        public NotifyJobCompleteService(IPushSubscriptionStore subscriptionStore,
            IPushNotificationService notificationService,
            INotificationRepository notificationRepository,
            ILogger<NotifyJobCompleteService> logger,
            IServiceProvider serviceProvider,
            IUnitOfWork unitOfWork) {
            _notificationService = notificationService;
            _subscriptionStore = subscriptionStore;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _handlers = serviceProvider.GetServices<INotificationHandler>().ToArray();
        }

        public async Task NotifyUser(string token, string userId, string title, string body, string target, string image) {
            var pushMessage = new PushMessage(body) {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            _logger.LogDebug("Sending GCM messages to subscribers");
            await _subscriptionStore.ForEachSubscriptionAsync(userId,
                subscription => {
                    _logger.LogDebug($"Sending to {target}");
                    _notificationService.SendNotificationAsync(subscription, pushMessage, target);
                });
        }

        public async Task SendCustomNotifications(string token, Guid podcastId, string title, string body, string url) {
            var id = podcastId.ToString();
            _logger.LogDebug($"Finding custom notifications for {id}");
            var notifications = _notificationRepository.GetAll().Where(r => r.PodcastId == podcastId);
            foreach (var notification in notifications) {
                _logger.LogDebug($"Found notification: {notification.Type.ToString()}");
                var typeHandlers = _handlers.Where(h => h.Type == notification.Type);
                foreach (var handler in typeHandlers) {
                    _logger.LogDebug($"Found handler: {notification.Type.ToString()}");
                    var response = await handler.SendNotification(notification.Id, title, body, url);
                    _logger.LogInformation(response);
                    _notificationRepository.AddLog(notification, response);
                    await _unitOfWork.CompleteAsync();
                }
            }
        }
    }
}
