using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public abstract class BaseNotificationHandler : INotificationHandler {
        public abstract Notification.NotificationType Type { get; }

        private readonly INotificationRepository _notificationRepository;
        protected readonly HttpClient _httpClient;

        protected BaseNotificationHandler(INotificationRepository notificationRepository,
            IHttpClientFactory httpClient) {
            _notificationRepository = notificationRepository;
            _httpClient = httpClient.CreateClient("Notifications");
        }

        public abstract Task<string> SendNotification(Guid notificationId, string userName, string title, string message, string url);

        protected async Task<Dictionary<string, string>> _getConfiguration(Guid notificationId) {
            var notification = await _notificationRepository.GetAsync(notificationId);
            if (notification is null) return null;

            var list = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(notification.Config);
            var dictionary = list.ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }
    }
}
