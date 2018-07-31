using System;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Jobs {
    public static class JobBootstrapper {
        public static void BootstrapJobs(bool isDevelopment) {
            if (isDevelopment)
                return;
            RecurringJob.AddOrUpdate<DeleteOrphanAudioJob>(x => x.Execute(), Cron.Daily(1));
            RecurringJob.AddOrUpdate<UpdateYouTubeDlJob>(x => x.Execute(), Cron.Daily(1, 30));
            RecurringJob.AddOrUpdate<ProcessPlaylistsJob>(x => x.Execute(), Cron.Daily(2));
            RecurringJob.AddOrUpdate<ProcessFailedPodcastsJob>(x => x.Execute(), Cron.Daily(2, 30));
            RecurringJob.AddOrUpdate<DeleteOrphanAudioJob>(x => x.Execute(), Cron.Daily(3));

            BackgroundJob.Schedule<ProcessRemoteAudioFileAttributesJob>(
                x => x.Execute(),
                TimeSpan.FromDays(Int16.MaxValue));
        }
    }
}
