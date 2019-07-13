using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Auth;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Push.Extensions;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Startup;

namespace PodNoms.Jobs {
    public class JobsStartup {
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Env { get; }

        public JobsStartup(IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine($"ApiConnection: \n\t{Configuration.GetConnectionString("PodNomsConnection")}");
            Console.WriteLine($"JobConnection: \n\t{Configuration.GetConnectionString("JobSchedulerConnection")}");
            Console.ReadKey();

            Console.WriteLine($"Configuring services");
            services.AddHangfire(options => {
                options.UseSqlServerStorage(Configuration.GetConnectionString("JobSchedulerConnection"));
                options.UseColouredConsoleLogProvider();
                options.UseSimpleAssemblyNameTypeSerializer();
                options.UseRecommendedSerializerSettings();
                //TODO: unsure if this is needed - re-enable if we get DI issues
                // options.UseActivator (new HangfireActivator (serviceProvider));
            });

            services
                .AddLogging()
                .AddPodNomsOptions(Configuration)
                .AddPodNomsMapping(Configuration)
                .AddSqlitePushSubscriptionStore(Configuration)
                .AddPushServicePushNotificationService()
                .AddDbContext<PodNomsDbContext>(options => {
                    options.UseSqlServer(Configuration.GetConnectionString("PodNomsConnection"));
                })
                .AddDependencies()
                .AddTransient<IRealTimeUpdater, SignalRClientUpdater>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireServer();
            Console.WriteLine("Configuring dashboard auth");
            app.UseHangfireDashboard("/hangfire", new DashboardOptions {
                Authorization = new[] { new HangFireAuthorizationFilter() }
            });

            JobBootstrapper.BootstrapJobs(false);
        }
    }
}
