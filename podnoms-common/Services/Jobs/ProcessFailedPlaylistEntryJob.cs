using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessFailedPlaylistEntryJob : IHostedJob {
        private readonly IUrlProcessService _processor;
        private readonly IRepository<ParsedPlaylistItem> _repository;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly ILogger<ProcessFailedPlaylistEntryJob> _logger;
        public ProcessFailedPlaylistEntryJob(ILogger<ProcessFailedPlaylistEntryJob> logger,
                IRepository<ParsedPlaylistItem> repository, IUrlProcessService processor, IEntryRepository entryRepository, IAudioUploadProcessService uploadService) {
            _logger = logger;
            _uploadService = uploadService;
            _repository = repository;
            _processor = processor;

        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            try {
                var entries = await _repository.GetAll()
                    .Where(r => r.IsProcessed == false)
                    .ToListAsync();
                foreach (var item in entries) {
                    BackgroundJob.Enqueue<ProcessPlaylistItemJob>(
                        service => service.Execute(item.Id.ToString(), item.PlaylistId, null));
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError($"Fatal error in ProcessFailedPodcastsJob");
                _logger.LogError(ex.Message);
            }
            return false;
        }
    }
}
