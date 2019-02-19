using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Text;
namespace PodNoms.Common.Services.Startup {
    public static class RoutingStartup {

        public static IServiceCollection AddPodNomsRouting(this IServiceCollection services,
               IConfiguration Configuration, bool isProduction) {
            return services;
        }
        public static IApplicationBuilder UsePodNomsRouting(this IApplicationBuilder builder) {
            return builder;

        }
    }
}