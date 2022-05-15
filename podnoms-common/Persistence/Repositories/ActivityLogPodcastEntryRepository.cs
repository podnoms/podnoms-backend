using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IActivityLogPodcastEntryRepository : IRepository<ActivityLogPodcastEntry> {
        Task<ActivityLogPodcastEntry> AddLogEntry(
            PodcastEntry entry,
            string referrer,
            string userAgent,
            string clientAddress,
            string extraInfo = "");

        Task<List<ActivityLogPodcastEntry>> GetForEntry(string entryId);
    }

    internal class ActivityLogPodcastEntryRepository : GenericRepository<ActivityLogPodcastEntry>,
        IActivityLogPodcastEntryRepository {
        public ActivityLogPodcastEntryRepository(
            PodNomsDbContext context,
            ILogger logger) : base(context, logger) {
        }

        public async Task<ActivityLogPodcastEntry> AddLogEntry(
            PodcastEntry entry, string referrer, string userAgent, string clientAddress, string extraInfo = "") {
            try {
                var log = new ActivityLogPodcastEntry {
                    PodcastEntry = entry,
                    Referrer = referrer,
                    UserAgent = userAgent,
                    ClientAddress = clientAddress,
                    ExtraInfo = extraInfo
                };
                this.Create(log);
                await GetContext().SaveChangesAsync();
                return log;
            } catch (Exception e) {
                _logger.LogError(15642, e, "Error logging podcast entry activity");
            }

            return null;
        }

        public async Task<List<ActivityLogPodcastEntry>> GetForEntry(string entryId) {
            return await GetAll()
                .Where(l => l.PodcastEntry.Id == Guid.Parse(entryId))
                .Include(a => a.PodcastEntry)
                .OrderByDescending(a => a.CreateDate)
                .ToListAsync();
        }
    }
}
