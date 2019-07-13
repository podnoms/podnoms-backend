using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodNoms.Common.Auth;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Gravatar;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Middleware;
using PodNoms.Common.Services.Notifications;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Payments;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Slack;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Startup {
    public static class Dependencies {
        public static IServiceCollection AddDependencies (this IServiceCollection services) {
            services.AddTransient<IFileUploader, AzureFileUploader> ()
                .AddTransient<IRealTimeUpdater, SignalRUpdater> ()
                .AddTransient<IPageParser, DefaultPageParser> ()
                .AddSingleton<IJwtFactory, JwtFactory> ()
                .AddSingleton<IUserIdProvider, SignalRUserIdProvider> ()
                .AddScoped<IUnitOfWork, UnitOfWork> ()
                .AddScoped<IPodcastRepository, PodcastRepository> ()
                .AddScoped<IEntryRepository, EntryRepository> ()
                .AddScoped<ICategoryRepository, CategoryRepository> ()
                .AddScoped<IPlaylistRepository, PlaylistRepository> ()
                .AddScoped<IChatRepository, ChatRepository> ()
                .AddScoped<INotificationRepository, NotificationRepository> ()
                .AddScoped<IPaymentRepository, PaymentRepository> ()
                .AddScoped<IDonationRepository, DonationRepository> ()
                .AddScoped<IUrlProcessService, UrlProcessService> ()
                .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService> ()
                .AddScoped<IAudioUploadProcessService, AudioUploadProcessService> ()
                .AddScoped<ISupportChatService, SupportChatService> ()
                .AddScoped<IMailSender, MailgunSender> ()
                .AddScoped<IFileUtilities, AzureFileUtilities> ()
                .AddScoped<INotificationHandler, SlackNotificationHandler> ()
                .AddScoped<INotificationHandler, IFTTNotificationHandler> ()
                .AddScoped<INotificationHandler, PushBulletNotificationHandler> ()
                .AddScoped<INotificationHandler, TwitterNotificationHandler> ()
                .AddScoped<INotificationHandler, EmailNotificationHandler> ()
                .AddScoped<IPaymentProcessor, StripePaymentProcessor> ()
                .AddScoped<YouTubeParser> ()
                .AddScoped<MixcloudParser> ()
                .AddScoped<UserLoggingFilter> ()
                .AddScoped<SlackSupportClient> ()
                .AddHttpClient<GravatarHttpClient> ();
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor> ();
            return services;
        }
    }
}
