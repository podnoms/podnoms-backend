using System;
using Hangfire;

namespace PodNoms.Common.Services.Jobs {
    public static class JobBootstrapper {
        public static void BootstrapJobs(bool isDevelopment) {
            if (!isDevelopment) {
                RecurringJob.AddOrUpdate<CheckAudioExistsJob>(x => x.Execute(null), Cron.Daily(1, 30));
                RecurringJob.AddOrUpdate<UpdateYouTubeDlJob>(x => x.Execute(null), Cron.Daily(2));
                RecurringJob.AddOrUpdate<CacheRemoteImageJob>(x => x.Execute(null), Cron.Daily(2, 30));
                RecurringJob.AddOrUpdate<ProcessMissingPodcastsJob>(x => x.Execute(null), Cron.Daily(3));

                RecurringJob.AddOrUpdate<DeleteOrphanAudioJob>(x => x.Execute(null), Cron.Monthly(1));

                RecurringJob.AddOrUpdate<ProcessPlaylistsJob>(x => x.Execute(), Cron.Yearly(1));

                RecurringJob.AddOrUpdate<DebugJobby>(x => x.Execute(null), Cron.Yearly(1));
                RecurringJob.AddOrUpdate<TagEntryJob>(x => x.Execute(null), Cron.Yearly(1));
                RecurringJob.AddOrUpdate<GenerateWaveformsJob>(x => x.Execute(null), Cron.Yearly(1));
                RecurringJob.AddOrUpdate<CheckItemImagesJob>(x => x.Execute(null), Cron.Yearly(1));
            }
        }
    }
}
