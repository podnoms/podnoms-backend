using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
                {"WebHookUrl (not currently enabled)", ""}
            };
        }
    }

    public class IFTTNotificationConfig : BaseNotificationConfig {
        public IFTTNotificationConfig() {
            Type = NotificationType.IFTT;
            Options = new Dictionary<string, string> {
                {"WebHookKey", ""},
                {"Event", ""},
            };
        }
    }

    public class GithubNotificationConfig : BaseNotificationConfig {
        public GithubNotificationConfig() {
            Type = NotificationType.Slack;
            Options = new Dictionary<string, string> {
                {"WebHookUrl (not currently enabled)", ""},
            };
        }
    }

    public class Notification : BaseEntity {
        public NotificationType Type { get; set; }
        public string Config { get; set; }

        public Guid PodcastId { get; set; }
        [JsonIgnore]
        public Podcast Podcast { get; set; }
    }
}