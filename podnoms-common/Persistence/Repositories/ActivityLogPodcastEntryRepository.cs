using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IActivityLogPodcastEntryRepository : IRepository<ActivityLogPodcastEntry> {
        Task<ActivityLogPodcastEntry> AddLogEntry(PodcastEntry entry, string clientAddress, string extraInfo = "");
    }
    public class ActivityLogPodcastEntryRepository : GenericRepository<ActivityLogPodcastEntry>, IActivityLogPodcastEntryRepository {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityLogPodcastEntryRepository(
            PodNomsDbContext context,
            ILogger<GenericRepository<ActivityLogPodcastEntry>> logger,
            IUnitOfWork unitOfWork) : base(context, logger) {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActivityLogPodcastEntry> AddLogEntry(PodcastEntry entry, string clientAddress, string extraInfo = "") {
            try {
                var log = new ActivityLogPodcastEntry {
                    PodcastEntry = entry,
                    ClientAddress = clientAddress,
                    ExtraInfo = extraInfo
                };
                this.Create(log);
                await _unitOfWork.CompleteAsync();
                return log;
            } catch (Exception e) {
                _logger.LogError($"Error logging podcast entry activity.\n{e.Message}");
            }
            return null;
        }
    }
}
