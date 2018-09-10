using System.Collections.Generic;

namespace PodNoms.Api.Models.Notifications {
    public class SlackNotificationConfig : BaseNotificationConfig {
        public SlackNotificationConfig() {
            Type = Notification.NotificationType.Slack;
            Options = new Dictionary<string, string> {
                {"WebHookUrl", ""},
                {"Channel", ""}
            };
        }
    }    
}