using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
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
        private readonly IEntryRepository _repository;
        private readonly IRepoAccessor _repoAccessor;
        private readonly ILogger<EntryPreProcessor> _logger;

        public EntryPreProcessor(
                    IOptions<StorageSettings> storageSettings,
                    IEntryRepository repository, IRepoAccessor repoAccessor,
                    ILogger<EntryPreProcessor> logger) {
            _storageSettings = storageSettings.Value;
            _repository = repository;
            _repoAccessor = repoAccessor;
            _logger = logger;
        }

        public async Task<EntryProcessResult> PreProcessEntry(ApplicationUser user, PodcastEntry entry) {
            var quota = user.DiskQuota ?? _storageSettings.DefaultUserQuota;
            var totalUsed = (await _repository.GetAllForUserAsync(user.Id))
                .Select(x => x.AudioFileSize)
                .Sum();

            if (totalUsed >= quota) {
                return EntryProcessResult.QuotaExceeded;
            }

            if (string.IsNullOrEmpty(entry.ImageUrl)) {
                entry.ImageUrl = $"{_storageSettings.CdnUrl}/static/images/default-entry.png";
            }

            entry.Processed = false;
            _repository.AddOrUpdate(entry);
            try {
                var succeeded = await _repoAccessor.CompleteAsync();
                if (succeeded) {
                    BackgroundJob.Enqueue<ProcessNewEntryJob>(e => e.ProcessEntry(entry.Id, null));
                    return EntryProcessResult.Succeeded;
                }
            } catch (DbUpdateException e) {
                _logger.LogError(e.Message);
            }
            return EntryProcessResult.GeneralFailure;
        }
    }
}
