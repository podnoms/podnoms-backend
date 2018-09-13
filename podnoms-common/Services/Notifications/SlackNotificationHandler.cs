using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class SlackNotificationHandler : BaseNotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.Slack;

        public SlackNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            var content = new StringContent(JsonConvert.SerializeObject(new {
                text = message
            }), Encoding.UTF8, "application/json");

            var config = await _getConfiguration(notificationId);
            if (config == null || !config.ContainsKey("WebHookUrl")) return false;
            var url = config["WebHookUrl"];
            var response = await _httpClient.PostAsync(
                url,
                content
            );
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}