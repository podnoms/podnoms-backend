using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public interface INotifyJobCompleteService {
        Task NotifyUser(string userId, string title, string body, string image);
        Task SendCustomNotifications(Guid podcastId, string title, string body, string url);
    }
}