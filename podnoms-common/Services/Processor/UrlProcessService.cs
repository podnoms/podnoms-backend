using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Hubs;
using PodNoms.Common.Services.Realtime;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class UrlProcessService : ProcessService, IUrlProcessService {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEntryRepository _repository;

        private readonly HelpersSettings _helpersSettings;
        private readonly HubLifetimeManager<UserUpdatesHub> _hub;

        public UrlProcessService(IEntryRepository repository, IUnitOfWork unitOfWork,
            IOptions<HelpersSettings> helpersSettings,
            HubLifetimeManager<UserUpdatesHub> hub,
            ILoggerFactory logger, IRealTimeUpdater realtimeUpdater, IMapper mapper)
            : base(logger, realtimeUpdater, mapper) {
            _helpersSettings = helpersSettings.Value;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _hub = hub;
        }

        private async Task __downloader_progress(string userId, string uid, ProcessProgressEvent e) {
            await _sendProgressUpdate(
                userId,
                uid,
                e);
        }

        public async Task<AudioType> GetInformation(string entryId) {
            var entry = await _repository.GetAsync(entryId);
            if (entry is null || string.IsNullOrEmpty(entry.SourceUrl)) {
                Logger.LogError("Unable to process item");
                return AudioType.Invalid;
            }

            if (entry.SourceUrl.EndsWith(".mp3") || entry.SourceUrl.EndsWith(".wav") ||
                entry.SourceUrl.EndsWith(".aif")) {
                return AudioType.Valid;
            }

            return await GetInformation(entry);
        }

        public async Task<AudioType> GetInformation(PodcastEntry entry) {
            var downloader = new AudioDownloader(entry.SourceUrl, _helpersSettings.Downloader);
            var ret = downloader.GetInfo();
            if (ret != AudioType.Valid) return ret;

            if (!string.IsNullOrEmpty(downloader.Properties?.Title)) {
                entry.Title = downloader.Properties?.Title;
            }

            entry.Description = downloader.Properties?.Description;
            entry.ImageUrl = downloader.Properties?.Thumbnail;
            entry.ProcessingStatus = ProcessingStatus.Processing;
            try {
                entry.Author = downloader.Properties?.Uploader;
            } catch (Exception) {
                Logger.LogWarning($"Unable to extract downloader info for: {entry.SourceUrl}");
            }

            await _unitOfWork.CompleteAsync();

            Logger.LogDebug("***DOWNLOAD INFO RETRIEVED****\n");
            Logger.LogDebug($"Title: {entry.Title}\nDescription: {entry.Description}\nAuthor: {entry.Author}\n");
            return ret;
        }

        public async Task<bool> DownloadAudio(Guid entryId) {
            var entry = await _repository.GetAsync(entryId);

            if (entry is null)
                return false;
            try {
                var downloader = new AudioDownloader(entry.SourceUrl, _helpersSettings.Downloader);
                var outputFile =
                    Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.mp3");

                downloader.DownloadProgress += async (s, e) => {
                    try {
                        await __downloader_progress(entry.Podcast.AppUser.Id, entry.Id.ToString(), e);
                    } catch (NullReferenceException nre) {
                        Logger.LogError(nre.Message);
                    }
                };

                downloader.PostProcessing += (s, e) => { Console.WriteLine(e); };
                var sourceFile = downloader.DownloadAudio(entry.Id);

                if (string.IsNullOrEmpty(sourceFile)) return true;

                entry.ProcessingStatus = ProcessingStatus.Uploading;
                entry.AudioUrl = sourceFile;

                await _sendProcessCompleteMessage(entry);
                await _unitOfWork.CompleteAsync();

                var updateMessage = new UserUpdatesHub.UserUpdateMessage
                {
                    Title = "Success",
                    Message = $"{entry.Title} has succesfully been processed",
                    ImageUrl = entry.ImageUrl
                };
                await _hub.SendUserAsync(
                    entry.Podcast.AppUser.Id,
                    "site-notices",
                    new object[] { updateMessage });
                return true;
            } catch (Exception ex) {
                Logger.LogError($"Entry: {entryId}\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                entry.ProcessingPayload = ex.Message;
                await _unitOfWork.CompleteAsync();
                await _sendProcessCompleteMessage(entry);
                await _hub.SendAllAsync(
                    entry.Podcast.AppUser.Id,
                    new object[] { $"Error processing {entry.Title}" });
            }

            return false;
        }
    }
}