using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class BaseNotificationConfig : INotificationConfig {
        public Notification.NotificationType Type { get; set; }
        public Dictionary<string, string> Options;
    }
}