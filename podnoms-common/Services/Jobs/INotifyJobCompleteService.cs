using System;
using System.Threading.Tasks;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public interface INotifyJobCompleteService {
        Task<bool> NotifyUser(string userId, string title, string body, string target, string image, NotificationOptions notificationType);
        Task SendCustomNotifications(Guid podcastId, string title, string body, string url);
    }
    public interface INotifyJobFailedService : INotifyJobCompleteService { }
}
