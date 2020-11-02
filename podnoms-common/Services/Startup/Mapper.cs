using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Data;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace PodNoms.Common.Services.Startup {
    public static class MapperStartup {
        private static Mutex mutex = new Mutex();

        public static IServiceCollection AddPodNomsMapping(this IServiceCollection services, IConfiguration config) {
            mutex.WaitOne();
            // Mapper.Reset();
            services.AddAutoMapper(
                e => { e.AddProfile(new MappingProvider(config)); },
                AppDomain.CurrentDomain.GetAssemblies());
            mutex.ReleaseMutex();
            return services;
        }
    }
}
