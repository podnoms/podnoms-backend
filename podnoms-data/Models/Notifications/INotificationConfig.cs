namespace PodNoms.Data.Models.Notifications {
    public interface INotificationConfig {
        Notification.NotificationType Type { get; set; }
    }
}