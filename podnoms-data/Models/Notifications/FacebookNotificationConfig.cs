using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class FacebookNotificationConfig : BaseNotificationConfig {
        public FacebookNotificationConfig() {
            Type = Notification.NotificationType.Facebook;
            //TODO: Fix this up
            Options = new Dictionary<string, NotificationOption> {
                {
                    "PageAccessToken",
                    new NotificationOption(
                        "PageAccessToken",
                        " Page access token",
                        "It will be challenging to create something that will post on behalf of any user, will revisit this when I have time",
                        true
                    )
                },
            };
        }
    }
}
