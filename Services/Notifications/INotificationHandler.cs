using System;
using System.Threading.Tasks;
using PodNoms.Api.Models.Notifications;

namespace PodNoms.Api.Services.Notifications {
    public interface INotificationHandler {
        Notification.NotificationType Type { get; }
        Task<bool> SendNotification(Guid notificationId, string title, string message);
    }
}