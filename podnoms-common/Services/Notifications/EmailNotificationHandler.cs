using System;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class EmailNotificationHandler : BaseNotificationHandler, INotificationHandler {
        private readonly IMailSender _emailSender;
        public override Notification.NotificationType Type => Notification.NotificationType.Email;

        public EmailNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient,
            IMailSender emailSender)
            : base(notificationRepository, httpClient) {
            _emailSender = emailSender;
        }

        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            var config = await _getConfiguration(notificationId);
            if (config == null || !(config.ContainsKey("To")))
                return false;

            var response = await _emailSender.SendEmailAsync(
                config["To"], 
                config["Subject"] ?? "New Podcast",
                $"Title: {title}\n\n{message}"
            );

            return true;
        }
    }
}