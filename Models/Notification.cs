using System;
using System.Collections.Generic;

namespace PodNoms.Api.Models {
    public enum NotificationType {
        Slack,
        Webhook,
        IFTT,
        Github
    }

    public interface INotificationConfig { }

    public class BaseNotificationConfig : INotificationConfig {
        public NotificationType Type { get; set; }
        public Dictionary<string, string> Options;
    }

    public class SlackNotificationConfig : BaseNotificationConfig {
        public SlackNotificationConfig() {
            Type = NotificationType.Slack;
            Options = new Dictionary<string, string> {
                {"WebHookUrl", ""},
                {"Channel", ""}
            };
        }
    }

    public class WebhookNotificationConfig : BaseNotificationConfig {
        public WebhookNotificationConfig() {
            Type = NotificationType.Webhook;
            Options = new Dictionary<string, string> {
                {"WebHookUrl", ""}
            };
        }
    }

    public class IFTTNotificationConfig : BaseNotificationConfig {
        public IFTTNotificationConfig() {
            Type = NotificationType.IFTT;
            Options = new Dictionary<string, string> {
                {"WebHookUrl", ""}
            };
        }
    }

    public class GithubNotificationConfig : BaseNotificationConfig {
        public GithubNotificationConfig() {
            Type = NotificationType.Slack;
            Options = new Dictionary<string, string> {
                {"WebHookUrl", ""},
                {"Channel", ""}
            };
        }
    }

    public class Notification : BaseEntity {
        public NotificationType Type { get; set; }
        public string Config { get; set; }

        public Guid PodcastId { get; set; }
        public Podcast Podcast { get; set; }
    }
}