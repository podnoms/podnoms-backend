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
using PodNoms.Common.Services.Push.Extensions;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Startup;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Jobs {
    public class JobsStartup {
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Env { get; }

        public JobsStartup (IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices (IServiceCollection services) {

            services.AddHangfire (options => {
                options.UseSqlServerStorage (Configuration.GetConnectionString ("JobSchedulerConnection"));
                options.UseColouredConsoleLogProvider ();
                options.UseSimpleAssemblyNameTypeSerializer ();
                options.UseRecommendedSerializerSettings ();
                // options.UseActivator (new HangfireActivator (serviceProvider));
            });

            services
                .AddLogging ()
                .AddPodNomsOptions (Configuration)
                .AddPodNomsMapping (Configuration)
                .AddSqlitePushSubscriptionStore (Configuration)
                .AddPushServicePushNotificationService ()
                .AddDbContext<PodNomsDbContext> (options => {
                    options.UseSqlServer (Configuration.GetConnectionString ("PodNomsConnection"));
                })
                .AddDependencies ()
                .AddTransient<IRealTimeUpdater, SignalRClientUpdater> ();
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }
            app.UseHangfireServer ();
            app.UseHangfireDashboard ();
            JobBootstrapper.BootstrapJobs (false);
        }
    }
}
