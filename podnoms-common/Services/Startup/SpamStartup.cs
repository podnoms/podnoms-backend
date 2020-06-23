using System;
using Akismet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Startup {
    public static class SpamStartup {
        public static IServiceCollection AddPodNomsSpamFilter(this IServiceCollection services, IConfiguration config) {
            services.AddSingleton(
                new AkismetClient(
                    config["SpamFilterSettings:AkismetKey"],
                    new Uri(config["SpamFilterSettings:BlogUrl"]),
                    "podnoms-spam-filter"
                )
            );
            return services;
        }
    }
}
