using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class EmailNotificationConfig : BaseNotificationConfig {
        public EmailNotificationConfig() {
            Type = Notification.NotificationType.Email;
            Options = new Dictionary<string, string> {
                {"FromName", ""},
                {"Subject", ""},
                {"To", ""},
                {"CC", ""},
                {"BCC", ""}
            };
        }
    }
}