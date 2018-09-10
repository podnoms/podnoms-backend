using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class IFTTNotificationConfig : BaseNotificationConfig {
        public IFTTNotificationConfig() {
            Type = Notification.NotificationType.IFTT;
            Options = new Dictionary<string, string> {
                {"WebHookKey", ""},
                {"Event", ""},
            };
        }
    }
}