using System;
using System.IO;
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
                            .AddEnvironmentVariables("ASPNETCORE_");
                        var builtConfig = config.Build();

                        Console.WriteLine($"Vault: {builtConfig["KeyVaultSettings:Vault"]}");
                        Console.WriteLine($"ClientId: {builtConfig["KeyVaultSettings:ClientId"]}");
                        Console.WriteLine($"ClientSecret: {builtConfig["KeyVaultSettings:ClientSecret"]}");

                        config.AddAzureKeyVault(
                            $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                            builtConfig["KeyVaultSettings:ClientId"],
                            builtConfig["KeyVaultSettings:ClientSecret"]);
                    }
                })
                .UseStartup<JobsStartup>();
    }
}
