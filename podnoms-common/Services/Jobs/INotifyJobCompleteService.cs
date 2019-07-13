using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public interface INotifyJobCompleteService {
        Task NotifyUser(string token, string userId, string title, string body, string target, string image);
        Task SendCustomNotifications(string token, Guid podcastId, string title, string body, string url);
    }
    public interface INotifyJobFailedService : INotifyJobCompleteService { }
}
