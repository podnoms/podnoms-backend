using System;
using System.Collections.Generic;

namespace PodNoms.Data.Models.Notifications {
    public class PushBulletNotificationConfig : BaseNotificationConfig {
        public PushBulletNotificationConfig() {
            Type = Notification.NotificationType.PushBullet;
            Options = new Dictionary<string, NotificationOption> {
                {
                    "AccessToken",
                    new NotificationOption("AccessToken", "Access Token",
                        "https://www.pushbullet.com/#settings/account", true)
                },
                {"Device", new NotificationOption("Device", "Device", "Blank for all or a device iden which can be found by sending an authenticated get to https://api.pushbullet.com/v2/devices", false)},
                {"Channel", new NotificationOption("Channel", "Channel", "Channel to send to", false)},
            };
        }
    }
}