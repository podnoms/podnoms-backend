using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Services.Waveforms;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessMissingPodcastsJob : AbstractHostedJob {
        private readonly IUrlProcessService _processor;
        private readonly IWaveformGenerator _waveFormGenerator;
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
            IWaveformGenerator waveFormGenerator,
            IEntryRepository entryRepository,
            IPlaylistRepository playlistRepository,
            IFileUtilities fileUtils,
            IAudioUploadProcessService uploadService) : base(logger) {
            _audioStorageSettings = audioStorageSettings.Value;
            _logger = logger;
            _uploadService = uploadService;
            _entryRepository = entryRepository;
            _playlistRepository = playlistRepository;
            _fileUtils = fileUtils;
            _processor = processor;
            _waveFormGenerator = waveFormGenerator;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> ExecuteForEntry(string entryId, PerformContext context) {
            var entry = await _entryRepository.GetAsync(entryId);
            if (entry != null) {
                await _process(entry.Id, entry.AudioUrl);
                return true;
            }
            return false;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            Log("Starting processing missing podcasts");
            try {
                var entries = await _entryRepository.GetAll()
                    // .Where(e => e.SourceUrl != string.Empty && e.SourceUrl != null)
                    // .Where(e => e.Processed == false)
                    .Where(e => e.Id == Guid.Parse("fde16ed1-2b56-4b38-7369-08d76d58dfa1"))
                    // .Include(e => e.Podcast)
                    // .Include(e => e.Podcast.AppUser)
                    .OrderByDescending(e => e.CreateDate)
                    .ToListAsync();
                foreach (var entry in entries) {
                    try {
                        Log($"Starting missingItem item:\n\t{entry.Id}\n\t{entry.Title}");
                        await _process(entry.Id, entry.AudioUrl);
                    } catch (Exception e) {
                        LogError($"Error processing item: {entry.Id}", e);
                    }
                }
                return true;
            } catch (Exception e) {
                LogError($"Fatal error in ProcessFailedPodcastsJob", e);
            }
            return false;
        }

        private async Task _process(Guid id, string audioUrl) {
            var audioExists = !string.IsNullOrEmpty(audioUrl) &&
                    await _fileUtils.CheckFileExists(audioUrl.Split('/')[0], audioUrl.Split('/')[1]);
            if (!audioExists) {
                Log($"Missing audio for: {id}");
                var localFile = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3");
                var processed = await _processor.DownloadAudio(string.Empty, id, localFile);
                if (processed) {
                    Log($"Processed: {id}");
                    Log($"Uploading audio for: {id}");
                    var uploaded = await _uploadService.UploadAudio(string.Empty, id, localFile);
                    if (!uploaded) {
                        LogError($"Error uploading audio from {id}");
                    }
                    var (json, dat, png) = await _waveFormGenerator.GenerateWaveformLocalFile(localFile);
                    if (!File.Exists(json)) {
                        LogError($"Error json does not exist {json}");
                    }
                } else {
                    LogError($"Unable to process podcast entry: {id}");
                }
            }
        }
    }
}
