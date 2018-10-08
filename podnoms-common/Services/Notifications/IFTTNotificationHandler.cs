using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class IFTTNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.IFTT;

        public IFTTNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<string> SendNotification(Guid notificationId, string title, string message, string url) {
            var config = await _getConfiguration(notificationId);
            if (config == null || !config.ContainsKey("WebHookKey") || !config.ContainsKey("Event")) 
                return "WebHookKey or Event missing in config";
            var hookUrl = $"https://maker.ifttt.com/trigger/{config["Event"]}/with/key/{config["WebHookKey"]}";
            var response = await _httpClient.GetAsync(hookUrl);
            return response.ToResponseString();
        }
    }
}