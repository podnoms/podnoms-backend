﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class IFTTTNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.IFTTT;

        public IFTTTNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<string> SendNotification(Guid notificationId, string userName, string title, string message, string url) {
            var config = await _getConfiguration(notificationId);
            if (config is null || !config.ContainsKey("WebHookKey") || !config.ContainsKey("Event"))
                return "WebHookKey or Event missing in config";
            var hookUrl = $"https://maker.ifttt.com/trigger/{config["Event"]}/with/key/{config["WebHookKey"]}";
            var response = await _httpClient.GetAsync(hookUrl);
            return response.ToResponseString();
        }
    }
}
