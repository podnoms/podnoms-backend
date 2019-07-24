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

        public override async Task<string> SendNotification(Guid notificationId, string userName, string title, string message, string url) {
            var config = await _getConfiguration(notificationId);
            if (config is null || !(config.ContainsKey("To")))
                return "\"To\" not found in config";

            var response = await _emailSender.SendEmailAsync(
                config["To"],
                config["Subject"] ?? "New Podcast",
                new MailDropin {
                    username = userName,
                    title = title,
                    message = $"Title: {title}\n\n{message}\n\n\n{url}"
                });

            return "Email succesfully sent to {config[\"To\"]}";
        }
    }
}
