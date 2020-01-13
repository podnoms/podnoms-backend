using System;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Notifications;
using PodNoms.Common.Services.Push;
using PodNoms.Data.Models;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Jobs {
    public class NotifyJobCompleteService : INotifyJobCompleteService {
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;

        private readonly ILogger<NotifyJobCompleteService> _logger;
        private readonly IMailSender _mailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHandler[] _handlers;
        private readonly IServiceScopeFactory _provider;
        public NotifyJobCompleteService(IPushSubscriptionStore subscriptionStore,
            IPushNotificationService notificationService,
            INotificationRepository notificationRepository,
            ILogger<NotifyJobCompleteService> logger,
            IServiceProvider serviceProvider,
            IMailSender mailSender,
            IServiceScopeFactory provider,
            IUnitOfWork unitOfWork) {
            _notificationService = notificationService;
            _subscriptionStore = subscriptionStore;
            _notificationRepository = notificationRepository;

            _logger = logger;
            _mailSender = mailSender;
            _unitOfWork = unitOfWork;
            _provider = provider;
            _handlers = serviceProvider.GetServices<INotificationHandler>().ToArray();
        }

        public async Task<bool> NotifyUser(string userId, string title, string body, string target, string image, NotificationOptions notificationType) {
            _logger.LogDebug($"Sending email messages to {userId}");
            await _sendEmail(userId, title, body, target, image, notificationType);
            var pushMessage = new PushMessage(body) {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            _logger.LogDebug($"Sending GCM messages to {userId}");
            await _subscriptionStore.ForEachSubscriptionAsync(userId,
                subscription => {
                    _logger.LogDebug($"Sending to {target}");
                    _notificationService.SendNotificationAsync(subscription, pushMessage, target);
                });
            return true;
        }

        private async Task<bool> _sendEmail(string userId, string title, string body, string target, string image, NotificationOptions notificationType) {
            try {
                _logger.LogDebug($"Locating services");
                using (IServiceScope scope = _provider.CreateScope()) {
                    _logger.LogDebug($"Finding user");
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null && (user.EmailNotificationOptions & notificationType) != 0) {
                        _logger.LogDebug($"User is {user.Email}");
                        //user has allowed this kinds of emails.
                        await _mailSender.SendEmailAsync(
                            user.Email,
                            $"New notification from PodNoms",
                            new MailDropin {
                                username = user.GetBestGuessName(),
                                title = title,
                                message = body,
                                buttonaction = target,
                                buttonmessage = "Check it out"
                            }
                        );
                    } else {
                        _logger.LogError($"Unable to find user");
                    }
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError(119157, ex, $"Error sending Notification email");
            }
            return false;
        }

        public async Task<bool> SendCustomNotifications(Guid podcastId, string userName, string title, string body, string url) {
            try {
                _logger.LogDebug($"Finding custom notifications for {podcastId}");
                var notifications = _notificationRepository.GetAll().Where(r => r.PodcastId == podcastId);
                foreach (var notification in notifications) {
                    _logger.LogDebug($"Found notification: {notification.Type.ToString()}");
                    var typeHandlers = _handlers.Where(h => h.Type == notification.Type);
                    foreach (var handler in typeHandlers) {
                        _logger.LogDebug($"Found handler: {notification.Type.ToString()}");
                        var response = await handler.SendNotification(
                            notification.Id,
                            userName,
                            title,
                            body,
                            url);
                        _logger.LogInformation(response);
                        _notificationRepository.AddLog(notification, response);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
            }
            return false;
        }
    }
}
