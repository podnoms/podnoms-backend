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
    public class GenerateWaveformsJob : IHostedJob {
        private readonly ILogger _logger;
        private readonly IEntryRepository _entryRepository;
        private readonly IFileUploader _fileUploader;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;
        private readonly IWaveformGenerator _waveFormGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateWaveformsJob(ILogger<GenerateWaveformsJob> logger,
                                    IEntryRepository entryRepository,
                                    IFileUploader fileUploader,
                                    IOptions<AppSettings> appSettings,
                                    IOptions<StorageSettings> storageSettings,
                                    IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
                                    IWaveformGenerator waveFormGenerator,
                                    IUnitOfWork unitOfWork) {
            _logger = logger;
            _entryRepository = entryRepository;
            _fileUploader = fileUploader;
            _appSettings = appSettings.Value;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
            _waveFormGenerator = waveFormGenerator;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Execute() { return await Execute(null); }

        public async Task<bool> Execute(PerformContext context) {
            _logger.LogInformation("Starting processing missing waveforms");
            var missingWaveforms = await _entryRepository.GetMissingWaveforms();

            foreach (var item in missingWaveforms) {
                _logger.LogInformation($"Processing waveform for: {item.Id}");
                BackgroundJob.Enqueue<GenerateWaveformsJob>(
                    r => r.ExecuteForEntry(item.Id, string.Empty, null)
                );
            }
            return true;
        }

        public async Task<bool> ExecuteForEntry(Guid entryId, string localFile, PerformContext context) {
            context.WriteLine($"Processing entry: {entryId}");
            if (!string.IsNullOrEmpty(localFile) && !File.Exists(localFile)) {
                _logger.LogError($"FileNotFound: {localFile}");
                return false;
            }
            var entry = await _entryRepository.GetAsync(entryId);
            if (entry != null) {
                _logger.LogInformation($"Generating waveform for: {entry.Id}");

                var (dat, json, png) = !string.IsNullOrEmpty(localFile) ?
                    await _waveFormGenerator.GenerateWaveformLocalFile(localFile) :
                    await _waveFormGenerator.GenerateWaveformRemoteFile(entry.GetAudioUrl(
                        _appSettings.AudioUrl, "mp3"));

                if (!string.IsNullOrEmpty(dat)) {
                    _logger.LogInformation("Uploading .dat");
                    await _fileUploader.UploadFile(
                        dat,
                        _waveformStorageSettings.ContainerName,
                        $"{entry.Id}.dat",
                        "application/x-binary", null);
                }
                if (!string.IsNullOrEmpty(json)) {
                    _logger.LogInformation("Uploading .json");
                    await _fileUploader.UploadFile(
                        json,
                        _waveformStorageSettings.ContainerName,
                        $"{entry.Id}.json",
                        "application/json", null);
                }
                if (!string.IsNullOrEmpty(png)) {
                    _logger.LogInformation("Uploading .png");
                    await _fileUploader.UploadFile(
                        png,
                        _waveformStorageSettings.ContainerName,
                        $"{entry.Id}.png",
                        "image/png", null);
                }
                entry.WaveformGenerated = true;
                _logger.LogInformation("Updating context");
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }
    }
}
