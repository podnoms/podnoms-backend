using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using PodNoms.Common.Data.Settings;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;

namespace PodNoms.Common.Services.Startup {

    public static class ImagingStartup {

        public static IServiceCollection AddPodNomsImaging (this IServiceCollection services, IConfiguration config) {
            var connectionString = config.GetSection ("StorageSettings") ["ConnectionString"];
            var containerName = config.GetSection ("ImageFileStorageSettings") ["ContainerName"];

            services.AddImageSharpCore ()
                .SetRequestParser<QueryCollectionRequestParser> ()
                .Configure<PhysicalFileSystemCacheOptions> (options => {
                    options.CacheFolder = "/tmp/pn-cache";
                })
                .SetCache<PhysicalFileSystemCache> ()
                .SetCacheHash<CacheHash> ()
                .AddProvider<AzureBlobStorageImageProvider> (AzureProviderFactory)
                .Configure<AzureBlobStorageImageProviderOptions> (options => {
                    options.ConnectionString = connectionString;
                    options.ContainerName = containerName;
                })
                .AddProcessor<ResizeWebProcessor> ();
            return services;
        }
        public static IApplicationBuilder UsePodNomsImaging (
            this IApplicationBuilder builder) {
            builder.UseImageSharp ();
            return builder;
        }

        private static AzureBlobStorageImageProvider AzureProviderFactory (IServiceProvider provider) {
            var containerName = provider.GetRequiredService<IOptions<ImageFileStorageSettings>> ().Value.ContainerName;

            return new AzureBlobStorageImageProvider (
                provider.GetRequiredService<IOptions<AzureBlobStorageImageProviderOptions>> (),
                provider.GetRequiredService<FormatUtilities> ()) {
                Match = context => {
                    return context.Request.Path.StartsWithSegments ($"/{containerName}");
                }
            };
        }
        private static PhysicalFileSystemProvider PhysicalProviderFactory (IServiceProvider provider) {
            return new PhysicalFileSystemProvider (
                provider.GetRequiredService<IHostingEnvironment> (),
                provider.GetRequiredService<FormatUtilities> ()) {
                Match = context => {
                    return !context.Request.Path.StartsWithSegments ("/local");
                }
            };
        }
    }
}
