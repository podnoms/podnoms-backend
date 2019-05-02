using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class SlackNotificationConfig : BaseNotificationConfig {
        public SlackNotificationConfig() {
            Type = Notification.NotificationType.Slack;
            Options = new Dictionary<string, NotificationOption> {
                {"WebHookUrl", new NotificationOption("WebHookUrl", "URL", "Your Slack incoming webhook URL", true)}, {
                    "Channel",
                    new NotificationOption("WebHookUrl", "URL",
                        "The Slack channel name (starting with '#') which will be used. Leave blank for webhook integration default.",
                        false)
                }
            };
        }
    }
}