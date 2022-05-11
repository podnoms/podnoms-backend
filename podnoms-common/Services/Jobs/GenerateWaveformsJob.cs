using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Services.Waveforms;

namespace PodNoms.Common.Services.Jobs {
    public class GenerateWaveformsJob : AbstractHostedJob {
        private readonly IFileUploader _fileUploader;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;
        private readonly IWaveformGenerator _waveFormGenerator;
        private readonly IRepoAccessor _repo;

        public GenerateWaveformsJob(ILogger logger,
            IFileUploader fileUploader,
            IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
            IWaveformGenerator waveFormGenerator,
            IRepoAccessor repo) : base(logger) {
            _fileUploader = fileUploader;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
            _waveFormGenerator = waveFormGenerator;
            _repo = repo;
        }

        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            Log("Starting processing missing waveforms");
            var missingWaveforms = await _repo.Entries.GetMissingWaveforms();

            foreach (var item in missingWaveforms) {
                Log($"Processing waveform for: {item.Id}");
                BackgroundJob.Enqueue<GenerateWaveformsJob>(
                    r => r.ExecuteForEntry(item.Id, string.Empty, null)
                );
            }

            return true;
        }

        public async Task<bool> ExecuteForEntry(Guid entryId, string localFile, PerformContext context) {
            _setContext(context);
            Log($"Creating waveform for: {entryId} using {localFile}");

            if (!string.IsNullOrEmpty(localFile) && !File.Exists(localFile)) {
                LogError($"FileNotFound: {localFile}");
                return false;
            }

            var entry = await _repo.Entries.GetAsync(entryId);
            if (entry == null) {
                return false;
            }

            Log($"Generating waveform for: {entry.Id}");

            var (dat, json, png) = !string.IsNullOrEmpty(localFile)
                ? await _waveFormGenerator.GenerateWaveformLocalFile(localFile)
                : await _waveFormGenerator.GenerateWaveformRemoteFile(
                    entry.GetRawAudioUrl(_storageSettings.CdnUrl, _audioFileStorageSettings.ContainerName, "mp3")
                );

            Log($"Dat: {dat}\nJSON: {json}\nPNG: {png}");
            if (!string.IsNullOrEmpty(dat)) {
                Log("Uploading .dat");
                await _fileUploader.UploadFile(
                    dat,
                    _waveformStorageSettings.ContainerName,
                    $"{entry.Id}.dat",
                    "application/x-binary", null);
            }

            if (!string.IsNullOrEmpty(json)) {
                Log("Uploading .json");
                await _fileUploader.UploadFile(
                    json,
                    _waveformStorageSettings.ContainerName,
                    $"{entry.Id}.json",
                    "application/json", null);
            }

            if (!string.IsNullOrEmpty(png)) {
                Log("Uploading .png");
                await _fileUploader.UploadFile(
                    png,
                    _waveformStorageSettings.ContainerName,
                    $"{entry.Id}.png",
                    "image/png", null);
            }

            entry.WaveformGenerated = true;
            Log("Updating context");
            await _repo.CompleteAsync();
            return true;
        }
    }
}
