using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodNoms.Common.Auth;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Gravatar;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Startup {
    public static class Dependencies {
        public static IServiceCollection AddDependencies(this IServiceCollection services) {
            services.AddTransient<IFileUploader, AzureFileUploader>()
                .AddTransient<IRealTimeUpdater, SignalRUpdater>()
                .AddSingleton<IJwtFactory, JwtFactory>()
                .AddSingleton<IUserIdProvider, SignalRUserIdProvider>()
                .AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<IPodcastRepository, PodcastRepository>()
                .AddScoped<IEntryRepository, EntryRepository>()
                .AddScoped<ICategoryRepository, CategoryRepository>()
                .AddScoped<IPlaylistRepository, PlaylistRepository>()
                .AddScoped<IUrlProcessService, UrlProcessService>()
                .AddScoped<INotifyJobCompleteService, NotifyJobCompleteService>()
                .AddScoped<IAudioUploadProcessService, AudioUploadProcessService>()
                .AddScoped<IChatRepository, ChatRepository>()
                .AddScoped<INotificationRepository, NotificationRepository>()
                .AddScoped<IMailSender, MailgunSender>()
                .AddScoped<IFileUtilities, AzureFileUtilities>()
                .AddHttpClient<GravatarHttpClient>();
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }
    }
}