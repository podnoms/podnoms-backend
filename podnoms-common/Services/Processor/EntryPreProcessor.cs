using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Jobs;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class EntryPreProcessor {
        public enum EntryProcessResult {
            Succeeded,
            QuotaExceeded,
            GeneralFailure
        }

        private readonly StorageSettings _storageSettings;
        private readonly IRepoAccessor _repo;
        private readonly ILogger<EntryPreProcessor> _logger;

        public EntryPreProcessor(
            IOptions<StorageSettings> storageSettings, IRepoAccessor repo,
            ILogger<EntryPreProcessor> logger) {
            _storageSettings = storageSettings.Value;
            _repo = repo;
            _logger = logger;
        }

        public async Task<EntryProcessResult> PreProcessEntry(ApplicationUser user, PodcastEntry entry) {
            var quota = user.DiskQuota ?? _storageSettings.DefaultUserQuota;
            var totalUsed = (await _repo.Entries.GetAllForUserAsync(user.Id))
                .Select(x => x.AudioFileSize)
                .Sum();

            if (totalUsed >= quota) {
                return EntryProcessResult.QuotaExceeded;
            }

            if (string.IsNullOrEmpty(entry.ImageUrl)) {
                entry.ImageUrl = $"{_storageSettings.CdnUrl}/static/images/default-entry.png";
            }

            entry.Processed = false;
            try {
                var succeeded = await _repo.CompleteAsync();
                if (succeeded) {
                    BackgroundJob.Enqueue<ProcessNewEntryJob>(e => e.ProcessEntry(entry.Id, null));
                    return EntryProcessResult.Succeeded;
                }
            } catch (DbUpdateException e) {
                _logger.LogError("{Message}", e.Message);
            }

            return EntryProcessResult.GeneralFailure;
        }
    }
}
