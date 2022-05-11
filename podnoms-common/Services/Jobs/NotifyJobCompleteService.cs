using System;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<NotifyJobCompleteService> _logger;
        private readonly IMailSender _mailSender;
        private readonly IRepoAccessor _repoAccessor;
        private readonly INotificationHandler[] _handlers;
        private readonly IServiceScopeFactory _provider;

        public NotifyJobCompleteService(IPushNotificationService notificationService,
            ILogger<NotifyJobCompleteService> logger,
            IServiceProvider serviceProvider,
            IMailSender mailSender,
            IServiceScopeFactory provider,
            IRepoAccessor repoAccessor) {
            _logger = logger;
            _mailSender = mailSender;
            _repoAccessor = repoAccessor;
            _provider = provider;
            _handlers = serviceProvider.GetServices<INotificationHandler>().ToArray();
        }

        public async Task<bool> NotifyUser(string userId, string title, string body, string target, string image,
            NotificationOptions notificationType) {
            _logger.LogDebug("Sending email messages to {UserId} - {Target}", userId, target);
            using var scope = _provider.CreateScope();

            // this is called from a rabbitmq service so we're on a different scope
            // DI has to be manually constucted
            var subscriptionStore = scope.ServiceProvider.GetRequiredService<IPushSubscriptionStore>();
            var notificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

            await _sendEmail(userId, title, body, target, image, notificationType);

            var pushMessage = new PushMessage(body) {
                Topic = title,
                Urgency = PushMessageUrgency.Normal
            };
            try {
                //TODO: Adding PushSubscriptionContext as Singleton is a recipe for disaster
                //TODO: but this gets fucked if I don't
                await subscriptionStore.ForEachSubscriptionAsync(userId,
                    subscription => {
                        _logger.LogInformation("Sending to {Target}", target);
                        notificationService.SendNotificationAsync(subscription, pushMessage, target);
                    });
                return true;
            } catch (Exception ex) {
                _logger.LogError(119155, ex, "Error notifying user");
            }

            return false;
        }

        private async Task<bool> _sendEmail(string userId, string title, string body, string target, string image,
            NotificationOptions notificationType) {
            try {
                _logger.LogDebug($"Locating services");
                using IServiceScope scope = _provider.CreateScope();
                _logger.LogDebug($"Finding user");
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(userId);
                if (user != null && (user.EmailNotificationOptions & notificationType) != 0) {
                    _logger.LogDebug("User is {User}", user.Email);
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

                return true;
            } catch (Exception ex) {
                _logger.LogError(119157, ex, "Error sending Notification email");
            }

            return false;
        }

        public async Task<bool> SendCustomNotifications(Guid podcastId, string userName, string title, string body,
            string url) {
            _logger.LogDebug("Finding custom notifications for {PodcastId}", podcastId);
            using IServiceScope scope = _provider.CreateScope();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var notifications = notificationRepository.GetAll().AsNoTracking()
                .Where(r => r.PodcastId == podcastId);
            foreach (var notification in notifications) {
                _logger.LogDebug("Found notification: {NotificationType}", notification.Type.ToString());
                try {
                    var typeHandlers = _handlers.Where(h => h.Type == notification.Type);
                    foreach (var handler in typeHandlers) {
                        _logger.LogDebug("Found handler: {Handler}", notification.Type.ToString());
                        var response = await handler.SendNotification(
                            notification.Id,
                            userName,
                            title,
                            body,
                            url);
                        _logger.LogInformation("{Response}", response);
                        notificationRepository.AddLog(notification, response);
                        await _repoAccessor.CompleteAsync();
                    }
                } catch (Exception ex) {
                    _logger.LogError(119157, ex, "Error sending custom notification");
                }
            }

            return true;
        }
    }
}
