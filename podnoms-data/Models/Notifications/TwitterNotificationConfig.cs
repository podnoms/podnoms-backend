using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class TwitterNotificationConfig : BaseNotificationConfig {
        public TwitterNotificationConfig() {
            Type = Notification.NotificationType.Twitter;
            Options = new Dictionary<string, string> {
                {"ConsumerKey", ""},
                {"ConsumerSecret", ""},
                {"AccessToken", ""},
                {"AccessTokenSecret", ""},
            };
        }
    }
}