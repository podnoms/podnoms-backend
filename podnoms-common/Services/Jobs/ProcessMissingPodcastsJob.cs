using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessMissingPodcastsJob : IJob {
        private readonly IUrlProcessService _processor;
        private readonly IEntryRepository _entryRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IFileUtilities _fileUtils;
        private readonly IAudioUploadProcessService _uploadService;
        private readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<ProcessMissingPodcastsJob> _logger;
        public ProcessMissingPodcastsJob(
            IOptions<AudioFileStorageSettings> audioStorageSettings,
            ILogger<ProcessMissingPodcastsJob> logger,
            IUrlProcessService processor,
            IEntryRepository entryRepository,
            IPlaylistRepository playlistRepository,
            IFileUtilities fileUtils,
            IAudioUploadProcessService uploadService) {
            _audioStorageSettings = audioStorageSettings.Value;
            _logger = logger;
            _uploadService = uploadService;
            _entryRepository = entryRepository;
            _playlistRepository = playlistRepository;
            _fileUtils = fileUtils;
            _processor = processor;
        }
        public async Task<bool> Execute() {
            try {
                var entries = await _entryRepository.GetAll()
                    .Where(e => e.SourceUrl != string.Empty && e.SourceUrl != null)
                    .Include(e => e.Podcast)
                    .Include(e => e.Podcast.AppUser)
                    .OrderByDescending(e => e.CreateDate)
                    .ToListAsync();
                foreach (var entry in entries) {
                    await _process(entry.Id, entry.AudioUrl);
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError($"Fatal error in ProcessFailedPodcastsJob");
                _logger.LogError(ex.Message);
            }
            return false;
        }

        private async Task _process(Guid id, string audioUrl) {
            var audioExists = await _fileUtils.CheckFileExists(audioUrl.Split('/')[0], audioUrl.Split('/')[1]);
            if (!audioExists) {
                var processed = await _processor.DownloadAudio(string.Empty, id);
                if (processed) {
                    var uploaded = await _uploadService.UploadAudio(string.Empty, id, audioUrl);
                    if (!uploaded) {
                        _logger.LogError($"Error uploading audio from {id}");
                    }
                } else {
                    _logger.LogError($"Unable to process podcast entry: {id}");
                }
            }
        }
    }
}
