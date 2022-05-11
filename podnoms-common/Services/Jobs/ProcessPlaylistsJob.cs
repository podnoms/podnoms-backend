using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistsJob : AbstractHostedJob {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IEntryRepository _entryRepository;
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly IYouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepoAccessor _repoAccessor;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;

        public ProcessPlaylistsJob(
            ILogger<ProcessPlaylistsJob> logger,
            UserManager<ApplicationUser> userManager,
            IPlaylistRepository playlistRepository,
            IEntryRepository entryRepository,
            IRepoAccessor repoAccessor,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<AppSettings> appSettings,
            IYouTubeParser youTubeParser,
            MixcloudParser mixcloudParser) : base(logger) {
            _userManager = userManager;
            _repoAccessor = repoAccessor;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _youTubeParser = youTubeParser;
            _mixcloudParser = mixcloudParser;
            _playlistRepository = playlistRepository;
            _entryRepository = entryRepository;
            _storageSettings = storageSettings.Value;
            _appSettings = appSettings.Value;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            Log("Starting playlist processing");
            context.WriteLine("Starting playlist processing");
            var playlists = await _playlistRepository.GetAll()
                // .Where(r => r.Id == Guid.Parse("49c8d76d-05a9-489f-991b-08d830faf155"))
                .ToListAsync();

            foreach (var playlist in playlists) {
                await Execute(playlist.Id, context);
            }

            return true;
        }

        public async Task<bool> Execute(Guid playlistId, PerformContext context) {
            Log($"Starting playlist processing for {playlistId}");
            context.WriteLine($"Starting playlist processing for {playlistId}");
            try {
                var playlist = await _playlistRepository.GetAsync(playlistId);
                var cutoffDate = await _playlistRepository.GetCutoffDate(playlistId);
                var user = playlist.Podcast.AppUser;

                //first check user has a valid subscription
                var subs = user.GetCurrentSubscription();
                var isGod = await _userManager.IsInRoleAsync(user, "god-mode");
                if (subs is null && !isGod) {
                    LogWarning($"User: {user.Id} does not have a valid subscription");
                    return false;
                }

                //next check quotas
                Log("Checking quotas");
                var quota = user.DiskQuota ?? _storageSettings.DefaultUserQuota;
                var totalUsed = (await _entryRepository.GetAllForUserAsync(user.Id))
                    .Select(x => x.AudioFileSize)
                    .Sum();

                if (totalUsed >= quota) {
                    LogError($"Storage quota exceeded for {user.GetBestGuessName()}");
                    BackgroundJob.Enqueue<INotifyJobCompleteService>(
                        service => service.NotifyUser(
                            user.Id.ToString(),
                            $"Failure processing playlist\n{playlist.Podcast.Title}\n",
                            $"Your have exceeded your storage quota of {quota.Bytes().ToString()}",
                            playlist.Podcast.GetAuthenticatedUrl(_appSettings.SiteUrl),
                            playlist.Podcast.GetThumbnailUrl(_storageSettings.CdnUrl,
                                _imageFileStorageSettings.ContainerName),
                            NotificationOptions.StorageExceeded
                        ));
                    return false;
                }

                Log("Quotas passed");
                //check for active subscription
                var resultList = new List<ParsedItemResult>();
                var count = user.PlaylistAllowedEntryCount ?? _storageSettings.DefaultEntryCount;

                if (_youTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    Log("Parsing YouTube");
                    var url = playlist.SourceUrl;
                    resultList = await _youTubeParser
                        .GetEntries(
                            url,
                            user.Id,
                            cutoffDate,
                            count);
                } else if (MixcloudParser.ValidateUrl(playlist.SourceUrl)) {
                    Log("Parsing MixCloud");
                    var entries = await _mixcloudParser
                        .GetEntries(playlist.SourceUrl, count);
                    resultList = entries
                        .OrderBy(r => r.UploadDate)
                        .Take(_storageSettings.DefaultEntryCount)
                        .ToList();
                }

                Log($"Found {resultList.Count} candidates");

                //order in reverse so the newest item is added first
                foreach (var item in resultList.Where(item =>
                    playlist.PodcastEntries.All(e => e.SourceItemId != item.Id))) {
                    await _trimPlaylist(playlist, count);
                    Log($"Found candidate\n\tParsedId:{item.Id}\n\tPodcastId:{playlist.Podcast.Id}\n\t{playlist.Id}");
                    BackgroundJob
                        .Enqueue<ProcessPlaylistItemJob>(
                            service => service.Execute(item, playlist.Id, null)
                        );
                }

                Log($"Finished playlists");
                return true;
            } catch (PlaylistExpiredException) {
                //TODO: Remove playlist and notify user
                LogError($"Playlist: {playlistId} cannot be found");
            } catch (Exception ex) {
                LogError(ex.Message);
                context.WriteLine($"ERROR(ProcessPlayListJob): {ex.Message}");
            }

            return false;
        }

        private async Task _trimPlaylist(Playlist playlist, int count) {
            var currentCount = playlist.PodcastEntries.Count(r => playlist.PodcastEntries.Contains(r));
            if (currentCount >= _storageSettings.DefaultEntryCount) {
                LogError($"Entry count exceeded for {playlist.Podcast.AppUser.GetBestGuessName()}");
                var toDelete = playlist.PodcastEntries
                    .OrderByDescending(o => o.SourceCreateDate)
                    .Take(currentCount - count + 1);

                foreach (var item in toDelete) {
                    await _entryRepository.DeleteAsync(item.Id);
                }

                await _repoAccessor.CompleteAsync();
            }
        }
    }
}
