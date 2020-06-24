using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using PodNoms.Common.Services.Hubs;
using EntitySignal.Extensions;
using EntitySignal.Hubs;

namespace PodNoms.Common.Services.Startup {
    public static class SignalRStartup {
        public static IServiceCollection AddPodNomsSignalR(this IServiceCollection services, bool isDevelopment) {
            services.AddSignalR(options => {
                options.EnableDetailedErrors = isDevelopment;
            }).AddJsonProtocol();

            services.AddEntitySignal();

            return services;
        }

        public static IApplicationBuilder UsePodNomsSignalRRoutes(
            this IApplicationBuilder app) {
            app.UseEndpoints(routes => {
                routes.MapHub<AudioProcessingHub>("/hubs/audioprocessing");
                routes.MapHub<UserUpdatesHub>("/hubs/userupdates");
                routes.MapHub<PublicUpdatesHub>("/hubs/publicupdates");
                routes.MapHub<DebugHub>("/hubs/debug");
                routes.MapHub<ChatHub>("/hubs/chat");
                routes.MapHub<EntityUpdatesHub>("/hubs/rtd");
            });
            return app;
        }
    }
}
