using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Jobs {
    public class CheckItemImagesJob : AbstractHostedJob, IHostedJob {
        private readonly IRepoAccessor _repo;
        private readonly RemoteImageCacher _imageCacher;
        private readonly IYouTubeParser _youTubeParser;

        public CheckItemImagesJob(ILogger<CheckItemImagesJob> logger,
            IRepoAccessor repo,
            RemoteImageCacher imageCacher, IYouTubeParser youTubeParser, IMailSender mailSender) : base(logger) {
            _repo = repo;
            _imageCacher = imageCacher;
            _youTubeParser = youTubeParser;
        }

        public override async Task<bool> Execute(PerformContext context) {
            _setPerformContext(context);
            Log("Starting CheckItemImagesJob");

            //get all the unprocessed images
            var entries = await _repo.Entries.GetAll()
                .Where(x => x.ImageUrl.StartsWith("http"))
                .ToListAsync();
            var count = entries.Count;
            int i = 1;
            foreach (var entry in entries) {
                Log($"Checking entry: {entry}");
                Log($"\t{i++} of {count}");

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

                await _repo.CompleteAsync();
            }

            return true;
        }
    }
}
