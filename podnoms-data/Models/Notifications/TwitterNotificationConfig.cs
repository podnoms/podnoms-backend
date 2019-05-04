using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class TwitterNotificationConfig : BaseNotificationConfig {
        public TwitterNotificationConfig() {
            Type = Notification.NotificationType.Twitter;
            Options = new Dictionary<string, NotificationOption> {
                {
                    "ConsumerKey",
                    new NotificationOption("ConsumerKey", "Consumer Key", "Your Twitter consumer key", true)
                }, {
                    "ConsumerSecret",
                    new NotificationOption("ConsumerSecret", "Consumer Secret", "Your Twitter consumer secret", true)
                }, {
                    "AccessToken",
                    new NotificationOption("AccessToken", "Access Token", "Your Twitter access token", true)
                }, {
                    "AccessTokenSecret",
                    new NotificationOption("AccessTokenSecret", "Access Token Secret",
                        "Your Twitter access token secret", true)
                },
            };
        }
    }
}