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
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Jobs {
    public class CheckItemImagesJob : AbstractHostedJob, IHostedJob {
        public readonly IEntryRepository _entryRepository;
        private readonly IRepoAccessor _repoAccessor;
        private readonly RemoteImageCacher _imageCacher;
        private readonly IYouTubeParser _youTubeParser;
        public readonly StorageSettings _storageSettings;
        public readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly IMailSender _mailSender;

        public CheckItemImagesJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, ILogger<CheckItemImagesJob> logger, IRepoAccessor repoAccessor,
            RemoteImageCacher imageCacher, IYouTubeParser youTubeParser, IMailSender mailSender) : base(logger) {
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _entryRepository = entryRepository;
            _repoAccessor = repoAccessor;
            _imageCacher = imageCacher;
            _youTubeParser = youTubeParser;
        }

        public override async Task<bool> Execute(PerformContext context) {
            _setContext(context);
            Log("Starting CheckItemImagesJob");

            //get all the unprocessed images
            var entries = await _entryRepository.GetAll()
                .Where(x => x.ImageUrl.StartsWith("http"))
                .ToListAsync();
            var count = entries.Count();
            int i = 1;
            foreach (var entry in entries) {
                Log($"Checking entry: {entry.ToString()}");
                Log($"\t{i++} of {count}");

                //https://cdn-l.podnoms.com/images/entry/d1260873-5d9f-4548-0524-08d6f73bf76c.png?width=725&height=748

                if (entry.ImageUrl.StartsWith("https://cdn-l.podnoms.com/")) {
                    entry.ImageUrl = entry.ImageUrl.Replace("https://cdn-l.podnoms.com/", string.Empty);
                    if (entry.ImageUrl.Contains('?')) {
                        entry.ImageUrl = entry.ImageUrl.Split('?')[0];
                    }
                } else {
                    var file = await _imageCacher.CacheImage(entry.ImageUrl, entry.Id.ToString());
                    if (string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(entry.SourceUrl)) {
                        if (_youTubeParser.ValidateUrl(entry.SourceUrl)) {
                            Log($"YouTube gave us a 404: {entry.ImageUrl}");
                            var info = await _youTubeParser.GetVideoInformation(
                                entry.SourceUrl,
                                entry.Podcast.AppUserId);
                            if (info != null) {
                                Log($"Parser gave us: {info} - attempting a cache");
                                file = await _imageCacher.CacheImage(info.Thumbnail, entry.Id.ToString());
                            }
                        } else {
                            Log("This isn't a YouTube - we'll deal with it later");
                        }
                    }
                    if (!string.IsNullOrEmpty(file)) {
                        Log("Happy Days!!!");
                        entry.ImageUrl = file;
                    }
                }
                await _repoAccessor.CompleteAsync();
            }
            return true;
        }
    }
}
