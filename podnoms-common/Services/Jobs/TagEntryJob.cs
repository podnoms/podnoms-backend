using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.AudioParsing.Helpers;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Audio;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class TagEntryJob : AbstractHostedJob {
        private readonly IRepoAccessor _repo;
        private readonly IFileUploader _fileUploader;
        private readonly ImageFileStorageSettings _imageStorageOptions;
        private readonly StorageSettings _storageOptions;
        private readonly AudioFileStorageSettings _audioStorageOptions;
        private readonly IMP3Tagger _tagger;

        public TagEntryJob(
            ILogger<TagEntryJob> logger,
            IRepoAccessor repo,
            IFileUploader fileUploader,
            IOptions<ImageFileStorageSettings> imageStorageOptions,
            IOptions<AudioFileStorageSettings> audioStorageOptions,
            IOptions<StorageSettings> storageOptions,
            IMP3Tagger tagger) : base(logger) {
            _repo = repo;
            _fileUploader = fileUploader;
            _imageStorageOptions = imageStorageOptions.Value;
            _audioStorageOptions = audioStorageOptions.Value;
            _storageOptions = storageOptions.Value;
            _tagger = tagger;
        }

        public override async Task<bool> Execute(PerformContext context) {
            this._setPerformContext(context);

            var entries = await this._repo.Entries.GetAll()
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
                        PathUtils.GetScopedTempFile());
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
                    await _repo.CompleteAsync();
                }
            }

            return false;
        }

        public async Task<bool> ExecuteForEntry(Guid entryId, string localFile,
            bool updateEntry, PerformContext context) {
            this._setPerformContext(context);
            Log($"Tagging entry: {entryId} using {localFile}");

            var entry = await _repo.Entries.GetAsync(entryId);
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
                        localImageFile =
                            await HttpUtils.DownloadFile("https://cdn.podnoms.com/static/images/pn-back.jpg");
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
                entry.AudioLength = _tagger.GetDuration(localFile);

                if (updateEntry) {
                    entry.MetadataStatus = 1;
                    await _repo.CompleteAsync();
                }
            } catch (Exception e) {
                this.LogError($"Error tagging entry: {e.Message}");
            }

            return true;
        }
    }
}
