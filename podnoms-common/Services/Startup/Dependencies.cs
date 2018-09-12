using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Gravatar;
using PodNoms.Api.Services.Jobs;
using PodNoms.Api.Services.Processor;
using PodNoms.Api.Services.Realtime;
using PodNoms.Api.Services.Storage;
using PodNoms.Common.Services;

namespace PodNoms.Services.Services.Startup {
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