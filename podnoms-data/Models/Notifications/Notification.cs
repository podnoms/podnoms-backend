using System;

namespace PodNoms.Data.Models.Notifications {
    public class Notification : BaseEntity {
        
        public enum NotificationType {
            Slack,
            IFTT,
            Email,
            Twitter,
            Facebook,        
            WebHook
        }
        
        public NotificationType Type { get; set; }
        public string Config { get; set; }

        public Guid PodcastId { get; set; }
        public Podcast Podcast { get; set; }
    }
}