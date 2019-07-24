using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class PushBulletNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.PushBullet;

        public PushBulletNotificationHandler(INotificationRepository notificationRepository,
            IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<string>
            SendNotification(Guid notificationId, string userName, string title, string message, string url) {
            var config = await _getConfiguration(notificationId);
            if (config is null || !config.ContainsKey("AccessToken"))
                return "Access token missing in config";

            var payload = JsonConvert.SerializeObject(new {
                device_iden = config["Device"] ?? string.Empty,
                title = title,
                body = message,
                type = "link",
                url = url
            });
            var hookUrl = "https://api.pushbullet.com/v2/pushes";
            _httpClient.DefaultRequestHeaders.Add("Access-Token", config["AccessToken"]);
            var response = await _httpClient.PostAsync(
                hookUrl,
                new StringContent(payload, Encoding.UTF8, "application/json"));
            return response.ToResponseString();
        }
    }
}
