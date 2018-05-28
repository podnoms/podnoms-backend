using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;

using PodNoms.Api.Persistence;
using PodNoms.Api.Providers;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api {
    public class Program {
        static bool isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development;

        public static void Main(string[] args) {
            var host = BuildWebHost(args);
            host.MigrateDatabase(true, false);
            host.Run();
        }
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => {
                if (!isDevelopment) {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddEnvironmentVariables();
                    var builtConfig = config.Build();
                    config.AddAzureKeyVault(
                        $"https://{builtConfig["Vault"]}.vault.azure.net/",
                        builtConfig["ClientId"],
                        builtConfig["ClientSecret"]);
                }
            })
            .UseStartup<Startup>()
            .UseUrls("http://0.0.0.0:5000")
            .UseKestrel(options => {
                options.Limits.MaxRequestBodySize = 1073741824;
            }).Build();

    }
}
