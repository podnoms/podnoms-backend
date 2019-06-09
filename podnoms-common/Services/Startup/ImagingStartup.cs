using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace PodNoms.Common.Services.Startup {
    public static class ImagingStartup {
        public static IServiceCollection cccccccAddPodNomsImaging (this IServiceCollection services, IConfiguration config) {
            return services;
            var connectionString = config.GetSection ("StorageSettings") ["ConnectionString"];
            var containerName = config.GetSection ("ImageFileStorageSettings") ["ContainerName"];
            services.AddImageSharp ()
                .Configure<AzureBlobStorageImageProviderOptions> (options => {
                    options.ConnectionString = connectionString;
                    options.ContainerName = containerName;
                })
                .AddProvider<AzureBlobStorageImageProvider> ()
                .AddProcessor<ResizeWebProcessor> ();;

            return services;
        }
        public static IApplicationBuilder UsePodNomsImaging (
            this IApplicationBuilder builder) {
            return builder;
            builder.UseImageSharp ();
            return builder;
        }
    }
}
