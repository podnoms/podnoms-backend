using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessFailedPodcastsJob : IHostedJob {
        private readonly IUrlProcessService _processor;
        private readonly IEntryRepository _entryRepository;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly ILogger<ProcessFailedPlaylistEntryJob> _logger;
        public ProcessFailedPodcastsJob(ILogger<ProcessFailedPlaylistEntryJob> logger, IUrlProcessService processor, IEntryRepository entryRepository, IAudioUploadProcessService uploadService) {
            _logger = logger;
            _uploadService = uploadService;
            _entryRepository = entryRepository;
            _processor = processor;

        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            try {
                var entries = await _entryRepository.GetAll()
                    .Include(e => e.Podcast)
                    .Include(e => e.Podcast.AppUser)
                    .Where(e => !e.Processed)
                    .ToListAsync();
                foreach (var entry in entries) {
                    var processed = await _processor.DownloadAudio(string.Empty, entry.Id);
                    if (processed) {
                        var uploaded = await _uploadService.UploadAudio(string.Empty, entry.Id, entry.AudioUrl);
                        if (!uploaded) {
                            _logger.LogError($"Error uploading audio from {entry.Id}");
                        }
                    } else {
                        _logger.LogError($"Unable to process podcast entry: {entry.Id}");
                    }
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
