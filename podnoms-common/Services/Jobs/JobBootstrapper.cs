using System;
using Hangfire;

namespace PodNoms.Common.Services.Jobs {
    public static class JobBootstrapper {
        public static void BootstrapJobs(bool isDevelopment) {
            if (!isDevelopment) {
                RecurringJob.AddOrUpdate<DeleteOrphanAudioJob>(x => x.Execute(), Cron.Daily(1));
                RecurringJob.AddOrUpdate<UpdateYouTubeDlJob>(x => x.Execute(), Cron.Daily(1, 30));
                RecurringJob.AddOrUpdate<ProcessPlaylistsJob>(x => x.Execute(), Cron.Yearly(1));
                RecurringJob.AddOrUpdate<CacheRemoteImageJob>(x => x.Execute(), Cron.Daily(3, 10));
                RecurringJob.AddOrUpdate<DebugJobby>(x => x.Execute(), Cron.Yearly(1));
                RecurringJob.AddOrUpdate<GenerateWaveformsJob>(x => x.Execute(), Cron.Yearly(1));
            }
        }
    }
}
