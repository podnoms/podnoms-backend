using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class EmailNotificationConfig : BaseNotificationConfig {
        public EmailNotificationConfig() {
            Type = Notification.NotificationType.Email;
            Options = new Dictionary<string, NotificationOption> {
                {"FromName", new NotificationOption("FromName", "From", "The name of the sender", true)},
                {"Subject", new NotificationOption("Subject", "Subject", "Subject line of the email", true)},
                {"To", new NotificationOption("To", "To", "The email address(es) of the recipients", true)},
                {"CC", new NotificationOption("CC", "CC", "The email address(es) to CC", false)},
                {"BCC", new NotificationOption("BCC", "BCC", "The email address(es) to BCC", false)}
            };
        }
    }
}