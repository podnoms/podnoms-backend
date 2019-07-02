using System;
using System.Configuration;
using System.IO;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Jobs {
    class Program {
        static void Main(string[] args) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var jobsConnectionString = configuration["ConnectionString:JobSchedulerConnection"];
            var podnomsConnectionString = configuration["ConnectionString:PodNomsConnection"];

            if (!string.IsNullOrEmpty(jobsConnectionString) && !string.IsNullOrEmpty(podnomsConnectionString)) {
                var serviceProvider = new ServiceCollection()
                    .AddLogging()
                    .AddDbContext<PodNomsDbContext>(options => {
                        options.UseSqlServer(podnomsConnectionString);
                    })
                    .AddScoped<IPodcastRepository, PodcastRepository>()
                    .AddScoped<IEntryRepository, EntryRepository>()
                    .AddScoped<ICategoryRepository, CategoryRepository>()
                    .AddScoped<IPlaylistRepository, PlaylistRepository>()
                    .AddScoped<IChatRepository, ChatRepository>()
                    .AddScoped<INotificationRepository, NotificationRepository>()
                    .AddScoped<IPaymentRepository, PaymentRepository>()
                    .AddScoped<IDonationRepository, DonationRepository>()
                    .AddScoped<IUrlProcessService, UrlProcessService>()
                    .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService>()
                    .AddScoped<IAudioUploadProcessService, AudioUploadProcessService>()
                    .BuildServiceProvider();


                GlobalConfiguration.Configuration
                  .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseColouredConsoleLogProvider()
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(jobsConnectionString, new SqlServerStorageOptions {
                      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                      QueuePollInterval = TimeSpan.Zero,
                      UseRecommendedIsolationLevel = true,
                      UsePageLocksOnDequeue = true,
                      DisableGlobalLocks = true
                  });

                BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
                JobBootstrapper.BootstrapJobs(false);
                
                using (var server = new BackgroundJobServer()) {
                    Console.ReadLine();
                }
            } else {
                Console.WriteLine("Unable to read connection string");
            }
        }
    }
}
