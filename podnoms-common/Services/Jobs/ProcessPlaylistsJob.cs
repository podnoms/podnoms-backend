﻿using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PodNoms.AudioParsing.UrlParsers;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessPlaylistsJob : AbstractHostedJob {
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly IYouTubeParser _youTubeParser;
        private readonly MixcloudParser _mixcloudParser;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepoAccessor _repo;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        private readonly JobSettings _jobSettings;

        public ProcessPlaylistsJob(
            ILogger<ProcessPlaylistsJob> logger,
            UserManager<ApplicationUser> userManager,
            IRepoAccessor repo,
            IOptions<StorageSettings> storageSettings,
            IOptions<JobSettings> jobSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IOptions<AppSettings> appSettings,
            IYouTubeParser youTubeParser,
            MixcloudParser mixcloudParser) : base(logger) {
            _userManager = userManager;
            _repo = repo;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _youTubeParser = youTubeParser;
            _mixcloudParser = mixcloudParser;
            _storageSettings = storageSettings.Value;
            _jobSettings = jobSettings.Value;
            _appSettings = appSettings.Value;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task<bool> Execute(PerformContext context) {
            Log("Starting playlist processing");
            context.WriteLine("Starting playlist processing");
            var playlists = await _repo.Playlists.GetAll()
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
                var playlist = await _repo.Playlists.GetAsync(playlistId);
                var cutoffDate = await _repo.Playlists.GetCutoffDate(playlistId);
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
                var totalUsed = (await _repo.Entries.GetAllForUserAsync(user.Id))
                    .Select(x => x.AudioFileSize)
                    .Sum();

                if (totalUsed >= quota && !isGod) {
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
                var count = isGod
                    ? Int32.MaxValue
                    : user.PlaylistAllowedEntryCount ?? _storageSettings.DefaultEntryCount;

                if (_youTubeParser.ValidateUrl(playlist.SourceUrl)) {
                    Log("Parsing YouTube");
                    var url = playlist.SourceUrl;
                    resultList = await _youTubeParser
                        .GetEntries(
                            url,
                            user.Id,
                            cutoffDate,
                            count);
                } else if (await new MixcloudPlaylistParser().IsMatch(playlist.SourceUrl)) {
                    Log("Parsing MixCloud");
                    var entries = await _mixcloudParser
                        .GetAllEntries(playlist.SourceUrl);
                    resultList = entries
                        .OrderBy(r => r.UploadDate)
                        .Take(isGod ? short.MaxValue : _storageSettings.DefaultEntryCount)
                        .ToList();
                }

                Log($"Found {resultList.Count} candidates");
                var toProcess = resultList.Where(item =>
                        playlist.PodcastEntries.All(e => e.SourceItemId != item.Id || e.Processed.Equals(false)))
                    //only take a certain number, otherwise we have hundreds of jobs
                    .Take(_jobSettings.MaxConcurrentPlaylistJobs)
                    .ToList();
                LogDebug($"{toProcess}");
                foreach (var item in toProcess.Take(1)) {
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
                    await _repo.Entries.DeleteAsync(item.Id);
                }

                await _repo.CompleteAsync();
            }
        }
    }
}
