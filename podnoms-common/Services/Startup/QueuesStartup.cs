using System;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Startup {
    public static class QueuesStartup {
        public static IServiceCollection AddPodnomsQueues(this IServiceCollection services, IConfiguration config) {
            Console.WriteLine($"Connecting to RabbitHutch: {config["RabbitMq:ConnectionString"]}");
            var bus = RabbitHutch.CreateBus(config["RabbitMq:ConnectionString"]);

            services.AddSingleton(bus);
            return services;
        }
    }
}
