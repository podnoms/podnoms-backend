using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class AudioUploadProcessService : RealtimeUpdatingProcessService, IAudioUploadProcessService {
        private readonly IEntryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploader _fileUploader;
        private readonly AudioFileStorageSettings _audioStorageSettings;

        public AudioUploadProcessService(IEntryRepository repository, IUnitOfWork unitOfWork,
            IFileUploader fileUploader, IOptions<AudioFileStorageSettings> audioStorageSettings,
            ILogger<AudioUploadProcessService> logger, IRealTimeUpdater realtimeUpdater, IMapper mapper)
            : base(logger, realtimeUpdater, mapper) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _fileUploader = fileUploader;
            _audioStorageSettings = audioStorageSettings.Value;
        }

        public async Task<bool> UploadAudio(string userId, Guid entryId, string localFile) {

            _logger.LogInformation($"Starting to upload audio for {entryId} - {localFile}");

            var entry = await _repository.GetAsync(entryId);
            if (entry == null) {
                _logger.LogError($"Unable to find entry with id: {entryId.ToString()}");
                return false;
            }
            entry.ProcessingStatus = ProcessingStatus.Uploading;
            await _sendProgressUpdate(
                    userId,
                    entry.Id.ToString(),
                    new ProcessingProgress(_mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry)) {
                        ProcessingStatus = ProcessingStatus.Uploading
                    });
            if (entry is null) {
                _logger.LogError($"Unable to find entry with id: {entryId}");
                return false;
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"Updated item status {entryId} - {localFile}");
            try {
                // TODO
                // bit messy but can't figure how to pass youtube-dl job result to this job
                // so using AudioUrl as a proxy
                if (string.IsNullOrEmpty(localFile))
                    localFile = entry.AudioUrl;

                if (File.Exists(localFile)) {
                    _logger.LogInformation($"Local item exists {entryId} - {localFile}");
                    var fileInfo = new FileInfo(localFile);
                    var fileName = $"{entry.Id.ToString()}{fileInfo.Extension}";

                    await _fileUploader.UploadFile(
                        localFile,
                        _audioStorageSettings.ContainerName,
                        fileName,
                        "application/mpeg",
                        async (p, t) => {
                            if (p % 1 == 0) {
                                try {
                                    await _sendProgressUpdate(
                                        userId,
                                        entry.Id.ToString(),
                                        new ProcessingProgress(new TransferProgress {
                                            Percentage = p,
                                            TotalSize = t.ToString()
                                        }) {
                                            ProcessingStatus = ProcessingStatus.Uploading
                                        });
                                } catch (Exception e) {
                                    _logger.LogError($"Error sending progress update.\n\t{e.Message}");
                                    _logger.LogError($"p:\np\n\n\n");
                                    _logger.LogError($"t:\n{ObjectDumper.Dump(t)}\n\n\n");
                                    _logger.LogError(ObjectDumper.Dump(entry));
                                }
                            }
                        });
                    entry.Processed = true;
                    entry.ProcessingStatus = ProcessingStatus.Processed;
                    entry.AudioUrl = $"{_audioStorageSettings.ContainerName}/{fileName}";
                    _logger.LogDebug($"AudioUrl is: {entry.AudioUrl}");
                    entry.AudioFileSize = fileInfo.Length;
                    await _unitOfWork.CompleteAsync();
                    var vm = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                    await _sendProgressUpdate(
                        userId,
                        entry.Id.ToString(),
                        new ProcessingProgress(entry) {
                            ProcessingStatus = ProcessingStatus.Processed,
                            Payload = vm
                        });
                    return true;
                }

                _logger.LogError($"Error uploading audio file: {entry.AudioUrl} does not exist");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                entry.ProcessingPayload = $"Unable to find {entry.AudioUrl}";
                await _unitOfWork.CompleteAsync();
                await _sendProgressUpdate(
                    userId,
                    entry.Id.ToString(),
                    new ProcessingProgress(entry) {
                        ProcessingStatus = ProcessingStatus.Failed
                    });
            } catch (Exception ex) {
                _logger.LogError($"Error uploading audio file: {ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                entry.ProcessingPayload = ex.Message;
                await _unitOfWork.CompleteAsync();
                await _sendProgressUpdate(
                    userId,
                    entry.Id.ToString(),
                    new ProcessingProgress(entry) {
                        ProcessingStatus = ProcessingStatus.Failed
                    });
            }
            return false;
        }
    }
}
