using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Persistence.Repositories {
    public interface INotificationRepository : IRepository<Notification> {
        NotificationLog AddLog(Notification notification, string response);
        Task<IList<NotificationLog>> GetLogsAsync(string notificationId);
    }

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository {
        public NotificationRepository(PodNomsDbContext context, ILogger<GenericRepository<Notification>> logger) : base(
            context, logger) { }

        public NotificationLog AddLog(Notification notification, string logText) {
            var log = new NotificationLog {
                Notification = notification,
                Log = logText
            };
            return GetContext().Add(log).Entity;
        }

        public async Task<IList<NotificationLog>> GetLogsAsync(string notificationId) {
            return await GetContext().NotificationLogs
                .Where(l => l.Notification.Id == Guid.Parse(notificationId))
                .ToListAsync();
        }
    }
}
