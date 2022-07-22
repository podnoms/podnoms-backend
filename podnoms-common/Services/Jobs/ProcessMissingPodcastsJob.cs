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
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Services.Waveforms;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessMissingPodcastsJob : AbstractHostedJob {
        private readonly IUrlProcessService _processor;
        private readonly IWaveformGenerator _waveFormGenerator;
        private readonly IFileUtilities _fileUtils;
        private readonly IFileUploader _fileUploader;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;
        private readonly IRepoAccessor _repo;
        private readonly IAudioUploadProcessService _uploadService;

        public ProcessMissingPodcastsJob(
            ILogger<ProcessMissingPodcastsJob> logger,
            IRepoAccessor repo,
            IUrlProcessService processor,
            IWaveformGenerator waveFormGenerator,
            IFileUtilities fileUtils,
            IFileUploader fileUploader,
            IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
            IAudioUploadProcessService uploadService) : base(logger) {
            _repo = repo;
            _uploadService = uploadService;
            _fileUtils = fileUtils;
            _fileUploader = fileUploader;
            _waveformStorageSettings = waveformStorageSettings.Value;
            _processor = processor;
            _waveFormGenerator = waveFormGenerator;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Debuggle(PerformContext context) {
            return await ExecuteForEntry("fde16ed1-2b56-4b38-7369-08d76d58dfa1", null);
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> ExecuteForEntry(string entryId, PerformContext context) {
            Log($"ExecuteForEntry: starting for entry {entryId}");
            var entry = await _repo.Entries.GetAsync(entryId);
            if (entry != null) {
                Log($"ExecuteForEntry: located entry {entryId}");
                await _process(entry.Id, entry.AudioUrl, true);
                return true;
            }

            Log($"ExecuteForEntry: failed to locate {entryId}");
            return false;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            Log("Starting processing missing podcasts");
            try {
                var entries = await _repo.Entries.GetAll()
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

        private async Task _process(Guid entryId, string audioUrl, bool forceReprocess = false) {
            var audioExists = !string.IsNullOrEmpty(audioUrl) &&
                              await _fileUtils.CheckFileExists(audioUrl.Split('/')[0], audioUrl.Split('/')[1]);
            if (!audioExists || forceReprocess) {
                //TODO: This is all largely a duplicate of ProcessEntryJob, should call into that...
                Log($"_process: Missing audio for: {entryId}");
                var localFile = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3");
                var processed = await _processor.DownloadAudio(entryId, localFile);
                if (processed) {
                    Log($"_process: Processed: {entryId}");
                    Log($"_process: Uploading audio for: {entryId}");
                    var uploaded = await _uploadService.UploadAudio(entryId, localFile);
                    if (!uploaded) {
                        LogError($"Error uploading audio from {entryId}");
                    }

                    Log($"_process: Generating waveform for: {entryId}");
                    var (dat, json, png) = await _waveFormGenerator.GenerateWaveformLocalFile(localFile);
                    if (!File.Exists(json)) {
                        LogError($"_process: Error json does not exist {json}");
                    } else {
                        Log($"_process: Uploading waveform for: {entryId}");
                        var result = await _fileUploader.UploadFile(
                            json,
                            _waveformStorageSettings.ContainerName,
                            $"{entryId}.json",
                            "application/x-binary",
                            null);
                        Log($"_process: Uploaded waveform for: {entryId}\n\tResult: {result}");
                    }

                    Log($"_process: Completed processing of: {entryId}");
                } else {
                    LogError($"_process: Unable to process podcast entry: {entryId}");
                }
            } else {
                Log($"_process: Audio exists, not processing");
            }
        }
    }
}
