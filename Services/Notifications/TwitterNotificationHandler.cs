using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Notifications;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Notifications {
    public class TwitterNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.Twitter;

        public TwitterNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }
        
        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            var config = await _getConfiguration(notificationId);
            if (config != null  && (config.ContainsKey("ConsumerKey") && config.ContainsKey("ConsumerSecret")
            ){
            var url = $"https://maker.ifttt.com/trigger/{config["Event"]}/with/key/{config["WebHookKey"]}";
            var response = await _httpClient.GetAsync(url);
            return response.StatusCode == HttpStatusCode.OK;
        }
            return false;
        }
    }
}