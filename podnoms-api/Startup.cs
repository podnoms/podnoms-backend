using System;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using EasyNetQ.Logging;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Hosted;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Middleware;
using PodNoms.Common.Services.Push.Extensions;
using PodNoms.Common.Services.Startup;
using PodNoms.Common.Utils;
using reCAPTCHA.AspNetCore;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Realtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PodNoms.Common.Services.Caching;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Common.Services.Rss;
using PodNoms.Data.Utils;
using Microsoft.AspNetCore.Identity;
using PodNoms.Data.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using PodNoms.Data.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PodNoms.Api {
    public class Startup {

        public static IConfiguration Configuration { get; private set; }
        public IWebHostEnvironment Env { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine($"Configuring services");
            services.AddResponseCaching();

            JobStorage.Current = new SqlServerStorage(
                Configuration["ConnectionStrings:JobSchedulerConnection"]
            );
            services.AddPodNomsMapping(Configuration);
            services.AddPodNomsOptions(Configuration);
            services.AddPodNomsHealthChecks(Configuration, Env.IsDevelopment());
            services.AddPodNomsCacheService(Configuration, true);

            Console.WriteLine($"Connecting to PodNoms db: {Configuration.GetConnectionString("DefaultConnection")}");
            services.AddDbContext<PodNomsDbContext>(options => {
                //FIXME Remove when https://github.com/aspnet/EntityFrameworkCore/issues/18943 is deployed
                options.ReplaceService<IMigrationsModelDiffer, ModelDiffer>();
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("podnoms-common")
                          .EnableRetryOnFailure());
            });

            Console.WriteLine($"Connecting to RabbitHutch: {Configuration["RabbitMq:ConnectionString"]}");
            services.AddSingleton<IBus>(RabbitHutch.CreateBus(Configuration["RabbitMq:ConnectionString"]));
            services.AddSingleton<AutoSubscriber>(provider =>
                new AutoSubscriber(
                    provider.GetRequiredService<IBus>(),
                    Assembly.GetExecutingAssembly().GetName().Name));

            Console.WriteLine($"Setting service scopes");

            services.AddScoped<CustomDomainRouteTransformer>();
            services.AddScoped<SharingLinkRouteTransformer>();
            services.AddHostedService<RabbitMQService>();
            services.AddPodNomsHttpClients(Configuration, Env.IsProduction());
            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);

            services.AddPodnomsSecurity(Configuration);
            services.AddPodNomsSignalR(Env.IsDevelopment());

            services.AddPodNomsAppInsights(Configuration, Env.IsProduction());

            services.AddPodNomsCors(Configuration);

            services.AddMvc(options => {
                //TODO: This needs to be investigated
                options.Filters.Add<UserLoggingFilter>();
                options.Filters.Add<UserLoggingFilter>();
                options.EnableEndpointRouting = false;
                options.OutputFormatters
                    .OfType<StringOutputFormatter>()
                    .Single().SupportedMediaTypes.Add("text/html");
            })
            .AddXmlSerializerFormatters()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PodNoms.API", Version = "v1" });
                c.DocumentFilter<LowercaseDocumentFilter>();
            });

            services.Configure<FormOptions>(x => {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            services.Configure<JsonOptions>(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });

            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));
            services.AddTransient<IRecaptchaService, RecaptchaService>();

            services.AddPodNomsSpamFilter(Configuration);

            services.AddPodNomsImaging(Configuration);
            services.AddPushSubscriptionStore(Configuration);
            services.AddPushNotificationService(Configuration);

            services.AddSharedDependencies()
                //the query service is orders of magnitude faster than the API 
                //but it will get rate limited if we use it on the job server
                .AddTransient<IYouTubeParser, YouTubeExplodeParser>()
                .AddTransient<IRealTimeUpdater, SignalRUpdater>()
                .AddScoped<RssFeedParser>()
                .AddScoped<UserLoggingFilter>()
                .AddScoped<ISupportChatService, SupportChatService>()
                .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService>(); //register this on it's own as the job server does it's own thing here..

            //register the codepages (required for slugify)
            var instance = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(instance);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider, IHostApplicationLifetime lifetime,
            UserManager<ApplicationUser> userManager) {

            UpdateDatabase(app, userManager, Configuration);

            if (!Env.IsDevelopment()) {
                app.UseHttpsRedirection();
            }

            app.UseSqlitePushSubscriptionStore();


            app.UseMessageQueue("ClientMessageService", Assembly.GetExecutingAssembly());

            //use the forwarded headers from nginx, not the proxyy headers
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            app.UsePodNomsImaging();
            app.UsePodNomsHealthChecks(Env.IsDevelopment());

            //TODO: Remove this and move to native JSON support
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseRobotsTxt(Env);
            app.UseResponseCaching();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UsePodNomsCors();
            app.UseSecureHeaders(Env.IsDevelopment());


            app.UsePodNomsSignalRRoutes();

            app.UseCustomDomainRedirect();

            app.UseEndpoints(endpoints => {
                endpoints.MapDynamicControllerRoute<SharingLinkRouteTransformer>("{shareId?}");
                endpoints.MapDynamicControllerRoute<CustomDomainRouteTransformer>("{**path}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PodNoms.API");
                c.RoutePrefix = "";
            });

        }

        private static void UpdateDatabase(IApplicationBuilder app, UserManager<ApplicationUser> userManager, IConfiguration config) {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<PodNomsDbContext>();
            context.Database.Migrate();
            PodNomsDbInitialiser.SeedUsers(userManager, config);
        }
    }
    public static class MessagingExtensions {
        public static IApplicationBuilder UseMessageQueue(this IApplicationBuilder appBuilder, string subscriptionIdPrefix, Assembly assembly) {
            var services = appBuilder.ApplicationServices.CreateScope().ServiceProvider;

            var lifeTime = services.GetService<IHostApplicationLifetime>();
            var bus = services.GetService<IBus>();
            lifeTime.ApplicationStarted.Register(() => {
                var subscriber = new AutoSubscriber(bus, subscriptionIdPrefix);
                subscriber.Subscribe(assembly);
                subscriber.SubscribeAsync(assembly);
            });
            lifeTime.ApplicationStopped.Register(() => bus.Dispose());
            return appBuilder;
        }
    }
}
