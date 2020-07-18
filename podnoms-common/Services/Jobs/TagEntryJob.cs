using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Audio;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class TagEntryJob : AbstractHostedJob {
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploader _fileUploader;
        private readonly ImageFileStorageSettings _imageStorageOptions;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageOptions;
        private readonly AudioFileStorageSettings _audioStorageOptions;
        private readonly IMP3Tagger _tagger;

        public TagEntryJob(
            ILogger<TagEntryJob> logger,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IFileUploader fileUploader,
            IOptions<AppSettings> appSettings,
            IOptions<ImageFileStorageSettings> imageStorageOptions,
            IOptions<AudioFileStorageSettings> audioStorageOptions,
            IOptions<StorageSettings> storageOptions,
            IMP3Tagger tagger) : base(logger) {
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
            _fileUploader = fileUploader;
            _imageStorageOptions = imageStorageOptions.Value;
            _audioStorageOptions = audioStorageOptions.Value;
            _appSettings = appSettings.Value;
            _storageOptions = storageOptions.Value;
            _tagger = tagger;
        }

        public override async Task<bool> Execute(PerformContext context) {
            this._setContext(context);

            var entries = await this._entryRepository.GetAll()
                .Include(e => e.Podcast)
                .Include(e => e.Podcast.AppUser)
                .Where(e => e.MetadataStatus == 0)
                // .Take(10)
                .ToListAsync();

            var count = entries.Count();
            var i = 1;
            foreach (var entry in entries) {
                try {
                    Log($"Processing {i++} of {count}");
                    Log($"Generating metadata for {entry.Title}");
                    var audioUrl = entry.GetAudioUrl($"{_storageOptions.CdnUrl}/{_audioStorageOptions.ContainerName}");
                    Log($"\tDownloading {audioUrl}");

                    var file = await HttpUtils.DownloadFile(
                        audioUrl,
                        System.IO.Path.Combine(
                            System.IO.Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3"));
                    if (!System.IO.File.Exists(file)) {
                        continue;
                    }

                    if (!await this.ExecuteForEntry(entry, file, false, context)) {
                        continue;
                    }

                    Log($"\tUploading {file}");

                    await _fileUploader.UploadFile(
                        file,
                        _audioStorageOptions.ContainerName,
                        $"{entry.Id.ToString()}.mp3",
                        "application/mpeg");
                    entry.MetadataStatus = 1;
                } catch (Exception ex) {
                    entry.MetadataStatus = -1;
                    LogError(ex.Message);
                } finally {
                    await _unitOfWork.CompleteAsync();
                }
            }
            Log("PREPARING SAVE!!");
            return false;
        }

        public async Task<bool> ExecuteForEntry(Guid entryId, string localFile,
            bool updateEntry, PerformContext context) {
            this._setContext(context);
            Log($"Tagging entry: {entryId} using {localFile}");

            var entry = await _entryRepository.GetAsync(entryId);
            return await ExecuteForEntry(entry, localFile, updateEntry, context);
        }

        public async Task<bool> ExecuteForEntry(PodcastEntry entry, string localFile,
            bool updateEntry, PerformContext context) {
            try {
                if (!System.IO.File.Exists(localFile)) {
                    return false;
                }

                var imageUrl = entry.GetImageUrl(_storageOptions.CdnUrl, _imageStorageOptions.ContainerName);
                var localImageFile = string.Empty;
                if (!string.IsNullOrEmpty(imageUrl)) {
                    localImageFile = await HttpUtils.DownloadFile(imageUrl);
                    if (!System.IO.File.Exists(localImageFile)) {
                        localImageFile = await HttpUtils.DownloadFile("https://cdn.podnoms.com/static/images/pn-back.jpg");
                    }
                }

                _tagger.CreateTags(
                    localFile,
                    localImageFile,
                    entry.Title,
                    entry.Podcast.Title,
                    entry.Podcast.AppUser.GetBestGuessName(),
                    $"Copyright © {System.DateTime.Now.Year} {entry.Podcast.AppUser.GetBestGuessName()}",
                    $"Robot Powered Podcasts from{Environment.NewLine}https://podnoms.com/");

                if (updateEntry) {
                    entry.MetadataStatus = 1;
                    await _unitOfWork.CompleteAsync();
                }
            } catch (Exception e) {
                this.LogError($"Error tagging entry: {e.Message}");
            }
            return true;
        }
    }
}
