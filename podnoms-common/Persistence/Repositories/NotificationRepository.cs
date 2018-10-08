using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Persistence.Repositories {
    public interface INotificationRepository : IRepository<Notification> {
        NotificationLog AddLog(Notification notification, string response);
    }

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository {
        public NotificationRepository(PodNomsDbContext context, ILogger<GenericRepository<Notification>> logger) : base(
            context, logger) { }

        public NotificationLog AddLog(Notification notification, string logText) {
            var log = new NotificationLog();
            log.Notification = notification;
            log.Log = logText;
            return GetContext().Add(log).Entity;
        }
    }
}