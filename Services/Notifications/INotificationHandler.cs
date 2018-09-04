using System;
using System.Threading.Tasks;

namespace PodNoms.Api.Services.Notifications {
    public interface INotificationHandler {
        Task<bool> SendNotification(Guid notificationId, string title, string message);
    }
}