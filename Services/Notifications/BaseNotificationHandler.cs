using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Notifications;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Notifications {
    public abstract class BaseNotificationHandler : INotificationHandler {
        public abstract Notification.NotificationType Type { get; }

        private readonly INotificationRepository _notificationRepository;
        protected readonly HttpClient _httpClient;

        protected BaseNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient) {
            this._notificationRepository = notificationRepository;
            this._httpClient = httpClient.CreateClient("Notifications");
        }
        public abstract Task<bool> SendNotification(Guid notificationId, string title, string message);

        protected async Task<Dictionary<string, string>> _getConfiguration(Guid notificationId) {
            var notification = await _notificationRepository.GetAsync(notificationId);
            if (notification == null) return null;
            
            var list = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(notification.Config);
            var dictionary = list.ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }
    }
}