using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class WebhookNotificationConfig : BaseNotificationConfig {
        public WebhookNotificationConfig() {
            Type = Notification.NotificationType.WebHook;
            Options = new Dictionary<string, string> {
                {"WebHookUrl (not currently enabled)", ""}
            };
        }
    }
}