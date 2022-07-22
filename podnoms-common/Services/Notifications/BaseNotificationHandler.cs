using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public abstract class BaseNotificationHandler : INotificationHandler {
        private readonly IRepoAccessor _repo;
        public abstract Notification.NotificationType Type { get; }

        protected readonly HttpClient _httpClient;

        protected BaseNotificationHandler(IRepoAccessor repo,
            IHttpClientFactory httpClient) {
            _repo = repo;
            _httpClient = httpClient.CreateClient("Notifications");
        }

        public abstract Task<string> SendNotification(Guid notificationId, string userName, string title,
            string message, string url);

        protected async Task<Dictionary<string, string>> _getConfiguration(Guid notificationId) {
            var notification = await _repo.Notifications.GetAsync(notificationId);
            if (notification is null) return null;

            var list = JsonSerializer.Deserialize<IEnumerable<KeyValuePair<string, string>>>(notification.Config);
            var dictionary = list.ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }
    }
}
