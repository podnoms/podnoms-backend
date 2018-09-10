namespace PodNoms.Api.Models.Notifications {
    public interface INotificationConfig {
        Notification.NotificationType Type { get; set; }
    }
}