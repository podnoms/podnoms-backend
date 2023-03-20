using System;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Startup {
    public static class QueuesStartup {
        public static IServiceCollection AddPodnomsQueues(this IServiceCollection services, IConfiguration config) {
            Console.WriteLine($"Connecting to RabbitHutch: {config["RabbitMq:ConnectionString"]}");
            services.RegisterEasyNetQ(config["RabbitMq:ConnectionString"],
                register => register.EnableMicrosoftLogging());
            var bus = RabbitHutch.CreateBus(config["RabbitMq:ConnectionString"]);
            services.AddSingleton(bus);
            return services;
        }
    }
}
