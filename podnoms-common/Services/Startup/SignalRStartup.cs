using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using PodNoms.Common.Services.Hubs;

namespace PodNoms.Common.Services.Startup {

    public static class SignalRStartup {
        public static IServiceCollection AddPodNomsSignalR(this IServiceCollection services) {
            services.AddSignalR()
                .AddJsonProtocol(options => options.PayloadSerializerSettings.ContractResolver =
                    new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                        {
                            ProcessDictionaryKeys = true
                        }
                    });

            return services;
        }

        public static IApplicationBuilder UsePodNomsSignalRRoutes(
            this IApplicationBuilder builder) {
            builder.UseSignalR(routes => {
                routes.MapHub<AudioProcessingHub>("/hubs/audioprocessing");
                routes.MapHub<UserUpdatesHub>("/hubs/userupdates");
                routes.MapHub<DebugHub>("/hubs/debug");
                routes.MapHub<ChatHub>("/hubs/chat");
            });
            return builder;
        }
    }
}