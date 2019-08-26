using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PodNoms.Jobs {
    public class Program {
        private static readonly bool _isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    if (context.HostingEnvironment.IsProduction()) {
                        Console.WriteLine("Production instance bootstrapping");
                        config.SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false)
                            .AddJsonFile($"appsettings.Production.json", optional: true)
                            .AddJsonFile("azurekeyvault.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables("ASPNETCORE_");
                        var builtConfig = config.Build();

                        config.AddAzureKeyVault(
                            $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                            builtConfig["KeyVaultSettings:ClientId"],
                            builtConfig["KeyVaultSettings:ClientSecret"]);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<JobsStartup>();
                });
    }
}
