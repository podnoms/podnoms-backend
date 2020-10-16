using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Jobs {
    public class CacheRemoteImageJob : IHostedJob {
        private readonly IEntryRepository _entryRepository;
        private readonly RemoteImageCacher _imageCacher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly StorageSettings _storageSettings;
        private readonly ILogger _logger;

        public CacheRemoteImageJob(IEntryRepository entryRepository,
            RemoteImageCacher imageCacher,
            IOptions<StorageSettings> storageSettings,
            IUnitOfWork unitOfWork,
            ILoggerFactory logger) {
            _entryRepository = entryRepository;
            _imageCacher = imageCacher;
            _unitOfWork = unitOfWork;
            _storageSettings = storageSettings.Value;
            _logger = logger.CreateLogger<CacheRemoteImageJob>();
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var images = _entryRepository
                .GetAll()
                .Where(r => r.Processed == true)
                .Where(r => r.Id == Guid.Parse("0FDDCF04-A8CC-4C9C-EC62-08D66614297B"));
            //.Where (r => r.ImageUrl.StartsWith ("https://i.ytimg.com/"));

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
            var entry = await _entryRepository.GetAsync(entryId);
            if (entry is null) return string.Empty;

            var file = await CacheImage(entry.ImageUrl, entry.Id);
            entry.ImageUrl = file;
            await _unitOfWork.CompleteAsync();
            return file;
        }
        public async Task<string> CacheImage(string imageUrl, Guid entryId) {

            var file = await _imageCacher.CacheImage(imageUrl, entryId.ToString());
            if (string.IsNullOrEmpty(file)) return string.Empty;

            return file;
        }
    }
}
