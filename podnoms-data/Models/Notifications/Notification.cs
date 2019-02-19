using System;

namespace PodNoms.Data.Models.Notifications {
    public class Notification : BaseEntity {
        public enum NotificationType {
            Slack,
            IFTT,
            Email,
            Twitter,
            Facebook,
            WebHook,
            PushBullet
        }

        public NotificationType Type { get; set; }
        public string Config { get; set; }

        public Guid PodcastId { get; set; }
        public Podcast Podcast { get; set; }
    }

    public class NotificationLog : BaseEntity {
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; }
        public string Log { get; set; }
    }
}