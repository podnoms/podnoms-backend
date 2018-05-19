using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Processor;

namespace PodNoms.Api.Services.Jobs {
    public class ProcessMissingPodcasts : IJob {
        private readonly IUrlProcessService _processor;
        private readonly IEntryRepository _entryRepository;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly ILogger<ProcessMissingPodcasts> _logger;
        public ProcessMissingPodcasts(ILogger<ProcessMissingPodcasts> logger, IUrlProcessService processor, IEntryRepository entryRepository, IAudioUploadProcessService uploadService) {
            this._logger = logger;
            this._uploadService = uploadService;
            this._entryRepository = entryRepository;
            this._processor = processor;

        }
        public async Task<bool> Execute() {
            try {
                var entries = await _entryRepository.GetAll()
                    .Include(e => e.Podcast)
                    .Include(e => e.Podcast.AppUser)
                    .Where(e => !e.Processed)
                    .ToListAsync();
                foreach (var entry in entries) {
                    var processed = await _processor.DownloadAudio(entry.Id);
                    if (processed) {
                        var uploaded = await this._uploadService.UploadAudio(entry.Id, entry.AudioUrl);
                        if (!uploaded) {
                            _logger.LogError($"Error uploading audio from {entry.Id}");
                        }
                    } else {
                        _logger.LogError($"Unable to process podcast entry: {entry.Id}");
                    }
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError($"Fatal error in ProcessMissingPodcasts");
                _logger.LogError(ex.Message);
            }
            return false;
        }
    }
}