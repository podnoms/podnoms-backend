using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class NotificationOption {
        public string Value { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string ControlType { get; set; }

        public NotificationOption(string key, string label, string description, bool required,
            string value = "", string controlType = "textbox") {
            Value = value;
            Key = key;
            Label = label;
            Description = description;
            Required = required;
            ControlType = controlType;
        }
    }

    public class BaseNotificationConfig : INotificationConfig {
        public Notification.NotificationType Type { get; set; }
        public Dictionary<string, NotificationOption> Options;


        public static BaseNotificationConfig GetConfig(string type) {
            switch (type) {
                case "Slack":
                    return new SlackNotificationConfig();
                case "IFTT":
                    return new IFTTNotificationConfig();
                case "Email":
                    return new EmailNotificationConfig();
                case "Twitter":
                    return new TwitterNotificationConfig();
                case "PushBullet":
                    return new PushBulletNotificationConfig();
                default:
                    return null;
            }
        }

        public static BaseNotificationConfig GetConfig(Notification.NotificationType type) {
            return GetConfig(type.ToString());
        }
    }
}