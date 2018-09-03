using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;
using PodNoms.Api.Utils;

namespace PodNoms.Api.Services.Notifications {
    public class SlackNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override NotificationType Type => NotificationType.Slack;

        public SlackNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            var content = new StringContent(JsonConvert.SerializeObject(new {
                text = message
            }).ToString(), Encoding.UTF8, "application/json");

            var config = await _getConfiguration(notificationId);
            if (config == null || !config.ContainsKey("WebHookUrl")) return false;
            var url = config["WebHookUrl"];
            var response = await this.httpClient.PostAsync(
                url,
                content
            );
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}