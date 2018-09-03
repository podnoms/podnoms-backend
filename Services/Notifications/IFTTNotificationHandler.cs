using System;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Notifications {
    public class IFTTNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override NotificationType Type => NotificationType.IFTT;

        public IFTTNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }
        
        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            // curl -X POST -H 'Content-type: application/json' \
            //      --data '{"text":"Hello, World!"}' \
            //      https://hooks.slack.com/services/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
            return true;
        }
    }
}