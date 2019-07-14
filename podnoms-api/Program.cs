using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PodNoms.Api {
    public class Program {
        private static readonly bool _isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development;

        public static void Main(string[] args) {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) {
            var builder = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    if (!_isDevelopment) {
                        config.SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false)
                            .AddJsonFile("azurekeyvault.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables("ASPNETCORE_");
                        var builtConfig = config.Build();

                        Console.WriteLine($"WE READ SOME SETTING {builtConfig["KeyVaultSettings:Vault"]}");
                        Console.WriteLine($"WE READ SOME SETTING {builtConfig["KeyVaultSettings:ClientId"]}");
                        Console.WriteLine($"WE READ SOME SETTING {builtConfig["KeyVaultSettings:ClientSecret"]}");

                        config.AddAzureKeyVault(
                            $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                            builtConfig["KeyVaultSettings:ClientId"],
                            builtConfig["KeyVaultSettings:ClientSecret"]);
                    }
                });
            var t = builder.UseStartup<Startup>()
                .UseKestrel(options => { options.Limits.MaxRequestBodySize = 1073741824; });

            return t.Build();
        }
    }
}
