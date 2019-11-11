using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Jobs.Geocoding;
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
    public class ActivityLogPodcastEntryRepository : GenericRepository<ActivityLogPodcastEntry>, IActivityLogPodcastEntryRepository {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityLogPodcastEntryRepository(
            PodNomsDbContext context,
            ILogger<GenericRepository<ActivityLogPodcastEntry>> logger,
            IUnitOfWork unitOfWork) : base(context, logger) {
            _unitOfWork = unitOfWork;
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
                await _unitOfWork.CompleteAsync();
                BackgroundJob.Enqueue<GeocodeActivityJob>(r => r.GeocodeActivityItem(log, null));
                return log;
            } catch (Exception e) {
                _logger.LogError($"Error logging podcast entry activity.\n{e.Message}");
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
