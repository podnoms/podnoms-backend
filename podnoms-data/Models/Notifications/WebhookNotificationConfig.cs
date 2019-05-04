using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class WebhookNotificationConfig : BaseNotificationConfig {
        public WebhookNotificationConfig() {
            Type = Notification.NotificationType.WebHook;
            Options = new Dictionary<string, NotificationOption>() {
                {
                    "WebHookUrl",
                    new NotificationOption("WebHookUrl", "URL", "POST request will be sent to this URL", false)
                }
            };
        }
    }
}