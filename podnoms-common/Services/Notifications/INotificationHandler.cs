using System;
using System.Threading.Tasks;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public interface INotificationHandler {
        Notification.NotificationType Type { get; }
        Task<string> SendNotification(Guid notificationId, string title, string message, string url);
    }
}