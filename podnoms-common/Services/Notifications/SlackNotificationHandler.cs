using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Common.Persistence;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class SlackNotificationHandler : BaseNotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.Slack;

        public SlackNotificationHandler(IRepoAccessor repo, IHttpClientFactory httpClient)
            : base(repo, httpClient) {
        }

        public override async Task<string> SendNotification(
            Guid notificationId, string userName,
            string title, string message, string url) {
            var content = new StringContent(JsonConvert.SerializeObject(new {
                text = $"Hello {userName}\n\n{message}\n{url}",
            }), Encoding.UTF8, "application/json");

            var config = await _getConfiguration(notificationId);
            if (config is null || !config.ContainsKey("WebHookUrl")) {
                return "WebHookUrl missing in config";
            }

            var hookUrl = config["WebHookUrl"];
            var response = await _httpClient.PostAsync(
                hookUrl,
                content
            );

            return response.ToResponseString();
        }
    }
}
