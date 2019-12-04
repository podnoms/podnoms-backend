using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class UrlProcessService : RealtimeUpdatingProcessService, IUrlProcessService {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AudioDownloader _downloader;
        private readonly IEntryRepository _repository;

        private readonly HelpersSettings _helpersSettings;

        public UrlProcessService(
            IEntryRepository repository, IUnitOfWork unitOfWork,
            IOptions<HelpersSettings> helpersSettings,
            AudioDownloader downloader,
            ILogger<UrlProcessService> logger, IRealTimeUpdater realtimeUpdater,
            IMapper mapper) : base(logger, realtimeUpdater, mapper) {
            _helpersSettings = helpersSettings.Value;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _downloader = downloader;
        }

        private async Task __downloader_progress(string userId, string uid, ProcessingProgress e) {
            if (!string.IsNullOrEmpty(userId)) {
                await _sendProgressUpdate(
                    userId,
                    uid,
                    e);
            }
        }

        public async Task<RemoteUrlType> GetInformation(string entryId) {
            var entry = await _repository.GetAsync(entryId);
            if (entry is null || string.IsNullOrEmpty(entry.SourceUrl)) {
                _logger.LogError("Unable to process item");
                return RemoteUrlType.Invalid;
            }

            if (entry.SourceUrl.EndsWith(".mp3") ||
                entry.SourceUrl.EndsWith(".wav") ||
                entry.SourceUrl.EndsWith(".aiff") ||
                entry.SourceUrl.EndsWith(".aif")) {
                return RemoteUrlType.SingleItem;
            }

            return await GetInformation(entry);
        }

        public async Task<RemoteUrlType> GetInformation(PodcastEntry entry) {

            var ret = await _downloader.GetInfo(entry.SourceUrl);
            if (!string.IsNullOrEmpty(_downloader.Properties?.Title) &&
                string.IsNullOrEmpty(entry.Title)) {
                entry.Title = _downloader.Properties?.Title;
            }

            entry.Description = _downloader.Properties?.Description ?? entry.Description;
            entry.ImageUrl = _downloader.Properties?.Thumbnail ?? entry.ImageUrl;
            entry.ProcessingStatus = ProcessingStatus.Processing;
            try {
                entry.Author = _downloader.Properties?.Uploader;
            } catch (Exception) {
                _logger.LogWarning($"Unable to extract downloader info for: {entry.SourceUrl}");
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogDebug("***DOWNLOAD INFO RETRIEVED****\n");
            _logger.LogDebug($"Title: {entry.Title}\nDescription: {entry.Description}\nAuthor: {entry.Author}\n");
            return ret;
        }

        public async Task<bool> DownloadAudio(string authToken, Guid entryId, string outputFile) {
            var entry = await _repository.GetAsync(entryId);

            if (entry is null)
                return false;
            try {
                await __downloader_progress(
                    authToken,
                    entry.Id.ToString(),
                    new ProcessingProgress(_mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry)) {
                        ProcessingStatus = ProcessingStatus.Processing
                    }
                );

                _downloader.DownloadProgress += async (s, e) => {
                    try {
                        await __downloader_progress(
                            authToken,
                            entry.Id.ToString(),
                            e
                        );
                    } catch (NullReferenceException nre) {
                        _logger.LogError(nre.Message);
                    }
                };

                var sourceFile = await _downloader.DownloadAudio(entry.Id, entry.SourceUrl, outputFile);

                if (string.IsNullOrEmpty(sourceFile)) return false;

                entry.ProcessingStatus = ProcessingStatus.Parsing;
                await _unitOfWork.CompleteAsync();

                return true;
            } catch (Exception ex) {
                _logger.LogError($"Entry: {entryId}\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                entry.ProcessingPayload = ex.Message;
                await _unitOfWork.CompleteAsync();
                await _sendProgressUpdate(
                    authToken,
                    entry.Id.ToString(),
                    new ProcessingProgress(entry) {
                        ProcessingStatus = ProcessingStatus.Processed
                    }
                );
            }

            return false;
        }
    }
}
