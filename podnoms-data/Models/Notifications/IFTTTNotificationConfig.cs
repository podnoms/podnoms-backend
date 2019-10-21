using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class IFTTTNotificationConfig : BaseNotificationConfig {
        public IFTTTNotificationConfig() {
            Type = Notification.NotificationType.IFTTT;
            Options = new Dictionary<string, NotificationOption> {
                {
                    "WebHookKey", new NotificationOption("WebHookKey", "Webhook key",
                        "Your IFTTT webhook key. You can get a key from https://ifttt.com/maker_webhooks", true)
                }, {
                    "Event", new NotificationOption("Event", "Event",
                        "The IFTTT maker event to fire", true)
                },
            };
        }
    }
}
