using System;
using System.Net;
using System.Net.Http;
using EasyNetQ;
using EasyNetQ.Logging;
using Hangfire;
using Hangfire.Console;
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
using PodNoms.Jobs.Services;

namespace PodNoms.Jobs {
    public class JobsStartup {
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Env { get; }
        private WebProxy localDebuggingProxy = new WebProxy("http://localhost:9537");

        public JobsStartup(IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services) {

            Console.WriteLine($"Configuring services");
            services.AddHangfire(options => {
                options.UseSqlServerStorage(Configuration.GetConnectionString("JobSchedulerConnection"));
                options.UseSimpleAssemblyNameTypeSerializer();
                options.UseRecommendedSerializerSettings();
                options.UseConsole();
                //TODO: unsure if this is needed - re-enable if we get DI issues
                // options.UseActivator (new HangfireActivator (serviceProvider));
            });

            services.AddHttpClient("podnoms", c => {
                c.BaseAddress = new Uri(Configuration["AppSettings:ApiUrl"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "PodNoms-JobProcessor");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                Proxy = localDebuggingProxy,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => {
                    return true;
                }
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
                .AddSingleton<IBus>(RabbitHutch.CreateBus(Configuration["RabbitMq:ConnectionString"]))
                .AddScoped<INotifyJobCompleteService, RabbitMqNotificationService>()
                .AddTransient<IRealTimeUpdater, SignalRClientUpdater>();
            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions {
                Authorization = new[] { new HangFireAuthorizationFilter() }
            });
            JobBootstrapper.BootstrapJobs(false);
        }
    }
}
