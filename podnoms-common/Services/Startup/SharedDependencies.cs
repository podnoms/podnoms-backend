using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodNoms.Common.Auth;
using PodNoms.Common.Auth.ApiKeys;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Audio;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Gravatar;
using PodNoms.Common.Services.Notifications;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Payments;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Slack;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Startup {
    public static class SharedDependencies {
        public static IServiceCollection AddSharedDependencies(this IServiceCollection services) {
            services.AddTransient<IFileUploader, AzureFileUploader>()
                .AddTransient<IPageParser, DefaultPageParser>()
                .AddTransient<IMP3Tagger, MP3Tagger>()
                .AddSingleton<IJwtFactory, JwtFactory>()
                .AddSingleton<IUserIdProvider, SignalRUserIdProvider>()
                .AddSingleton<IGetApiKeyQuery, IssuedKeysGetApiKeyQuery>()
                .AddScoped(typeof(IRepository<>), typeof(GenericRepository<>))
                .AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<IPodcastRepository, PodcastRepository>()
                .AddScoped<IEntryRepository, EntryRepository>()
                .AddScoped<IActivityLogPodcastEntryRepository, ActivityLogPodcastEntryRepository>()
                .AddScoped<ICategoryRepository, CategoryRepository>()
                .AddScoped<IPlaylistRepository, PlaylistRepository>()
                .AddScoped<IChatRepository, ChatRepository>()
                .AddScoped<INotificationRepository, NotificationRepository>()
                .AddScoped<IPaymentRepository, PaymentRepository>()
                .AddScoped<IDonationRepository, DonationRepository>()
                .AddScoped<IUrlProcessService, UrlProcessService>()
                .AddScoped<IAudioUploadProcessService, AudioUploadProcessService>()
                .AddScoped<IMailSender, MailgunSender>()
                .AddScoped<IFileUtilities, AzureFileUtilities>()
                .AddScoped<INotificationHandler, SlackNotificationHandler>()
                .AddScoped<INotificationHandler, IFTTTNotificationHandler>()
                .AddScoped<INotificationHandler, PushBulletNotificationHandler>()
                .AddScoped<INotificationHandler, TwitterNotificationHandler>()
                .AddScoped<INotificationHandler, EmailNotificationHandler>()
                .AddScoped<IPaymentProcessor, StripePaymentProcessor>()
                .AddScoped<MixcloudParser>()
                .AddScoped<AudioDownloader>()
                .AddScoped<SlackSupportClient>()
                .AddHttpClient<GravatarHttpClient>();
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }
    }
}
