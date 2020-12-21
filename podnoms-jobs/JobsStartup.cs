using System;
using EasyNetQ;
using EasyNetQ.Logging;
using Hangfire;
using Hangfire.Console;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Caching;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Push.Extensions;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Startup;
using PodNoms.Common.Services.Waveforms;
using PodNoms.Jobs.Services;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Common.Services.Social;
using HangfireBasicAuthenticationFilter;
using PodNoms.Common.Utils;

namespace PodNoms.Jobs {
    public class JobsStartup {
        public static IConfiguration Configuration { get; private set; }
        public IHostEnvironment Env { get; }

        public JobsStartup(IHostEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine($"Configuring services");
            services.AddHangfire(options => {
                options.UseSqlServerStorage(
                    Configuration.GetConnectionString("JobSchedulerConnection"),
                    new Hangfire.SqlServer.SqlServerStorageOptions {
                        QueuePollInterval = TimeSpan.FromSeconds(30)
                    });
                options.UseSimpleAssemblyNameTypeSerializer();
                options.UseRecommendedSerializerSettings();
                options.UseConsole();
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
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                })
                .AddPodnomsSecurity(Configuration)
                .AddPodNomsHttpClients(Configuration, Env.IsProduction())
                .AddPodNomsCacheService(Configuration, false)
                .AddPodNomsSignalR(Env.IsDevelopment())
                .AddSharedDependencies()
                .AddSingleton<IBus>(RabbitHutch.CreateBus(Configuration["RabbitMq:ExternalConnectionString"]))
                .AddSingleton<RemoteImageCacher>()
                .AddSingleton<ITweetListener, EpisodeFromTweetHandler>()
                .AddScoped<IYouTubeParser, YouTubeParser>()
                .AddScoped<IWaveformGenerator, AWFWaveformGenerator>()
                .AddScoped<INotifyJobCompleteService, RabbitMqNotificationService>()
                .AddScoped<CachedAudioRetrievalService, CachedAudioRetrievalService>()
                .AddScoped<IRealTimeUpdater, SignalRClientUpdater>();

            services.AddHostedService<TweetListenerService>();

            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireServer(new BackgroundJobServerOptions {
                WorkerCount = 5 //HardwareUtils.CPUAndCoreCount
            });

            app.UseHangfireDashboard("/dashboard", new DashboardOptions {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter{
                    User = Configuration.GetSection("HangfireDashboardSettings:UserName").Value,
                    Pass = Configuration.GetSection("HangfireDashboardSettings:Password").Value
                }}
            });
            app.Run(async (context) => {
                await context.Response.WriteAsync("Hello World!");
            });
            JobBootstrapper.BootstrapJobs(false);
        }
    }
}
