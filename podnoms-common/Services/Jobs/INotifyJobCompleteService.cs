using System;
using System.Threading.Tasks;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public interface INotifyJobCompleteService {
        Task<bool> NotifyUser(
            string userId, string title, string body,
            string target, string image, NotificationOptions notificationType);

        Task<bool> SendCustomNotifications(
            Guid podcastId, string userName,
            string title, string body, string url);
    }
    public interface INotifyJobFailedService : INotifyJobCompleteService { }
}
