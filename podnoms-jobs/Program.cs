using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PodNoms.Jobs {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
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
                .UseStartup<JobsStartup>();
    }
}
