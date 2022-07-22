using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class IFTTTNotificationHandler : BaseNotificationHandler, INotificationHandler {
        private readonly ILogger<IFTTTNotificationHandler> _logger;
        public override Notification.NotificationType Type => Notification.NotificationType.IFTTT;

        public IFTTTNotificationHandler(IRepoAccessor repo, IHttpClientFactory httpClient,
            ILogger<IFTTTNotificationHandler> logger)
            : base(repo, httpClient) {
            _logger = logger;
        }

        public override async Task<string> SendNotification(Guid notificationId, string userName, string title,
            string message, string url) {
            _logger.LogDebug("Sending IFTTT notification for {NotificationId} - {UserName} - {Title}",
                notificationId, userName, title);
            var config = await _getConfiguration(notificationId);
            if (config is null || !config.ContainsKey("WebHookKey") || !config.ContainsKey("Event"))
                return "WebHookKey or Event missing in config";
            var hookUrl = $"https://maker.ifttt.com/trigger/{config["Event"]}/with/key/{config["WebHookKey"]}";
            var response = await _httpClient.GetAsync(hookUrl);
            _logger.LogDebug(
                "Sent IFTTT notification for {NotificationId} - {UserName} - {Title}\n\tCode: {ResponseCode} - Body: {ResponseBody}",
                notificationId, userName, title, response.StatusCode, response.ToResponseString());
            return response.ToResponseString();
        }
    }
}
