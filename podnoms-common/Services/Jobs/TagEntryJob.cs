using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Audio;

namespace PodNoms.Common.Services.Jobs {
    public class TagEntryJob : AbstractHostedJob {
        private readonly IEntryRepository _entryRepository;
        private readonly ImageFileStorageSettings _imageStorageOptions;
        private readonly StorageSettings _storageOptions;
        private readonly IMP3Tagger _tagger;

        public TagEntryJob(
                ILogger<TagEntryJob> logger,
                IEntryRepository entryRepository,
                IOptions<ImageFileStorageSettings> imageStorageOptions,
                IOptions<StorageSettings> storageOptions,
                IMP3Tagger tagger) : base(logger) {
            _entryRepository = entryRepository;
            _imageStorageOptions = imageStorageOptions.Value;
            _storageOptions = storageOptions.Value;
            _tagger = tagger;
        }

        public override async Task<bool> Execute(PerformContext context) {
            this._setContext(context);
            return false;
        }

        public async Task<bool> ExecuteForEntry(Guid entryId, string localFile, PerformContext context) {
            var entry = await _entryRepository.GetAsync(entryId);
            if (!System.IO.File.Exists(localFile)) {
                return false;
            }

            await _tagger.CreateTags(
                localFile,
                entry.GetImageUrl(_storageOptions.CdnUrl, _imageStorageOptions.ContainerName),
                entry.Title,
                entry.Podcast.Title,
                entry.Podcast.AppUser.GetBestGuessName(),
                $"Copyright © {System.DateTime.Now.Year} {entry.Podcast.AppUser.GetBestGuessName()}",
                $"Robot Powered Podcasts from{Environment.NewLine}https://podnoms.com/");
            return true;
        }
    }
}
