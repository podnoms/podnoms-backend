using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Jobs {
    public class CacheRemoteImageJob : IHostedJob {
        private readonly IRepoAccessor _repo;
        private readonly RemoteImageCacher _imageCacher;
        private readonly IRepoAccessor _repoAccessor;
        private readonly ILogger _logger;

        public CacheRemoteImageJob(IRepoAccessor repo,
            RemoteImageCacher imageCacher,
            IRepoAccessor repoAccessor,
            ILoggerFactory logger) {
            _repo = repo;
            _imageCacher = imageCacher;
            _repoAccessor = repoAccessor;
            _logger = logger.CreateLogger<CacheRemoteImageJob>();
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var images = _repo.Entries
                .GetAll()
                .Where(r => r.Processed == true)
                .Where(r => r.Id == Guid.Parse("0FDDCF04-A8CC-4C9C-EC62-08D66614297B"));

            int i = 1;
            int count = images.Count();
            foreach (var e in images) {
                _logger.LogDebug($"Caching image for: {e.Id}");
                _logger.LogDebug($"Caching: {e.Id}");
                await CacheImage(e.Id);
                _logger.LogDebug($"Processing {i++} of {count}");
            }

            return true;
        }

        public async Task<string> CacheImage(Guid entryId) {
            var entry = await _repo.Entries.GetAsync(entryId);
            if (entry is null) return string.Empty;

            var file = await CacheImage(entry.ImageUrl, entry.Id);
            entry.ImageUrl = file;
            await _repoAccessor.CompleteAsync();
            return file;
        }

        private async Task<string> CacheImage(string imageUrl, Guid entryId) {
            var file = await _imageCacher.CacheImage(imageUrl, entryId.ToString());
            return string.IsNullOrEmpty(file) ? string.Empty : file;
        }
    }
}
