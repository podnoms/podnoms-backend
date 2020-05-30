using System;
using System.Collections.Generic;
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
using PodNoms.Api.Providers;
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
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCore3;
using WebMarkupMin.Core;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Api {
    public class Startup {

        public static IConfiguration Configuration { get; private set; }
        public IHostEnvironment Env { get; }

        public Startup(IHostEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine($"Configuring services");
            services.AddResponseCaching();
            services.AddWebMarkupMin(options => {
                options.AllowMinificationInDevelopmentEnvironment = true;
                options.AllowCompressionInDevelopmentEnvironment = true;
            })
                .AddHtmlMinification(options => {
                    HtmlMinificationSettings settings = options.MinificationSettings;
                    settings.RemoveRedundantAttributes = true;
                    settings.RemoveHttpProtocolFromAttributes = true;
                    settings.RemoveHttpsProtocolFromAttributes = true;
                })
                .AddHttpCompression();

            JobStorage.Current = new SqlServerStorage(
                Configuration["ConnectionStrings:JobSchedulerConnection"]
            );
            services.AddPodNomsMapping(Configuration);
            services.AddPodNomsOptions(Configuration);
            services.AddPodNomsHealthChecks(Configuration, Env.IsDevelopment());
            services.AddPodNomsCacheService(Configuration, true);

            services.AddDbContext<PodNomsDbContext>(options => {
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("podnoms-common"));
            });

            services.AddSingleton<IBus>(RabbitHutch.CreateBus(Configuration["RabbitMq:ConnectionString"]));
            services.AddSingleton<AutoSubscriber>(provider =>
                new AutoSubscriber(
                    provider.GetRequiredService<IBus>(),
                    Assembly.GetExecutingAssembly().GetName().Name));
            services.AddHostedService<RabbitMQService>();
            services.AddPodNomsHttpClients(Configuration, Env.IsProduction());
            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);

            services.AddPodnomsSecurity(Configuration);
            services.AddPodNomsSignalR(Env.IsDevelopment());

            services.AddPodNomsAppInsights(Configuration, Env.IsProduction());

            services.AddMvc(options => {
                //TODO: This needs to be investigated
                options.Filters.Add<UserLoggingFilter>();
                options.Filters.Add<UserLoggingFilter>();
                options.EnableEndpointRouting = false;
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
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

            services.AddPodNomsCors(Configuration);
            services.AddPodNomsImaging(Configuration);
            services.AddPushSubscriptionStore(Configuration);
            services.AddPushNotificationService(Configuration);

            services.AddSharedDependencies()
                //the query service is orders of magnitude faster than the API 
                //but it will get rate limited if we use it on the job server
                .AddTransient<IYouTubeParser, YouTubeQueryService>()
                .AddTransient<IRealTimeUpdater, SignalRUpdater>()
                .AddScoped<UserLoggingFilter>()
                .AddScoped<ISupportChatService, SupportChatService>()
                .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService>(); //register this on it's own as the job server does it's own thing here..

            //register the codepages (required for slugify)
            var instance = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(instance);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider, IHostApplicationLifetime lifetime) {

            UpdateDatabase(app);

            if (!Env.IsDevelopment()) {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(new ExceptionHandlerOptions {
                ExceptionHandler = new JsonExceptionMiddleware(Env).Invoke
            });

            app.UseSqlitePushSubscriptionStore();

            app.UseRouting();
            app.UseAuthorization();
            app.UseCustomDomainRewrites();
            app.UseStaticFiles();

            app.UseMessageQueue("ClientMessageService", Assembly.GetExecutingAssembly());

            //use the forwarded headers from nginx, not the proxyy headers
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();

            app.UsePodNomsCors();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PodNoms.API");
                c.RoutePrefix = "";
            });
            app.UsePodNomsImaging();
            app.UsePodNomsSignalRRoutes();
            app.UsePodNomsHealthChecks(Env.IsDevelopment());
            app.UseSecureHeaders(Env.IsDevelopment());

            //TODO: Remove this and move to native JSON support
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseRobotsTxt(Env);
            app.UseResponseCaching();
            app.UseWebMarkupMin();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "shared",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        private static void UpdateDatabase(IApplicationBuilder app) {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<PodNomsDbContext>();
            context.Database.Migrate();
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
