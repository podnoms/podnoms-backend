using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Jobs {
    public class Startup {
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Env { get; }

        public Startup (IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices (IServiceCollection services) {
            var jobsConnectionString = Configuration["ConnectionString:JobSchedulerConnection"];
            var podnomsConnectionString = Configuration["ConnectionString:PodNomsConnection"];

            services.AddHangfire (options => {
                options.UseSqlServerStorage (jobsConnectionString);
                options.UseColouredConsoleLogProvider ();
                options.UseSimpleAssemblyNameTypeSerializer ();
                options.UseRecommendedSerializerSettings ();
                // options.UseActivator (new HangfireActivator (serviceProvider));
            });

            services
                .AddLogging ()
                .AddDbContext<PodNomsDbContext> (options => {
                    options.UseSqlServer (podnomsConnectionString);
                })
                .AddScoped<IPodcastRepository, PodcastRepository> ()
                .AddScoped<IEntryRepository, EntryRepository> ()
                .AddScoped<ICategoryRepository, CategoryRepository> ()
                .AddScoped<IPlaylistRepository, PlaylistRepository> ()
                .AddScoped<IChatRepository, ChatRepository> ()
                .AddScoped<INotificationRepository, NotificationRepository> ()
                .AddScoped<IPaymentRepository, PaymentRepository> ()
                .AddScoped<IDonationRepository, DonationRepository> ()
                .AddScoped<IUrlProcessService, UrlProcessService> ()
                .AddScoped<IUnitOfWork, UnitOfWork> ()
                .AddTransient<IFileUploader, AzureFileUploader> ()
                .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService> ()
                .AddScoped<IAudioUploadProcessService, AudioUploadProcessService> ()
                .AddTransient<IRealTimeUpdater, SignalRUpdater> ();
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env, IRecurringJobManager recurringJobManager) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }
            app.UseHangfireServer ();
            app.UseHangfireDashboard ();
            JobBootstrapper.BootstrapJobs (false);
        }
    }
}
