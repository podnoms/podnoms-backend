using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Api.Persistence {
    public interface INotificationRepository : IRepository<Notification> {

    }
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository {
        public NotificationRepository(PodNomsDbContext context, ILogger<GenericRepository<Notification>> logger) : base(context, logger) {
        }
    }
}