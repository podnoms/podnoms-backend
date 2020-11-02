using System;

namespace PodNoms.Data.Models.Notifications {
    public class Notification : BaseEntity {
        public enum NotificationType {
            Slack,
            IFTTT,
            Email,
            Twitter,
            Facebook,
            WebHook,
            PushBullet,
            Other
        }

        public NotificationType Type { get; set; }
        public string Config { get; set; }

        public Guid PodcastId { get; set; }
        public virtual Podcast Podcast { get; set; }
    }

    public class NotificationLog : BaseEntity {
        public Guid NotificationId { get; set; }
        public virtual Notification Notification { get; set; }
        public string Log { get; set; }
        public bool Succeeded { get; set; }
    }
}
