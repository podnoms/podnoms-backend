using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
                    var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
                    if (!context.HostingEnvironment.IsProduction()) {
                        return;
                    }

                    Console.WriteLine("Production instance bootstrapping");
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{platform}.json", optional: true)
                        .AddJsonFile($"appsettings.Production.json", optional: true)
                        .AddJsonFile("azurekeyvault.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables("ASPNETCORE_");
                    var builtConfig = config.Build();

                    config.AddAzureKeyVault(
                        $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                        builtConfig["KeyVaultSettings:ClientId"],
                        builtConfig["KeyVaultSettings:ClientSecret"]);
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<JobsStartup>().UseKestrel(options => {
                        options.Limits.MaxRequestBodySize = 1073741824;
                        if (!_isDevelopment) {
                            return;
                        }

                        var c = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.Development.json", optional: false)
                            .AddEnvironmentVariables("ASPNETCORE_")
                            .Build();

                        var certificate = new X509Certificate2(
                            c[
                                $"DevSettings:CertificateFile{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "")}"],
                            c["DevSettings:CertificateSecret"]);

                        options.Listen(IPAddress.Any, 5003, listenOptions => {
                            listenOptions.UseHttps(certificate);
                        });
                        options.Listen(IPAddress.Any, 5002);
                    });
                });
    }
}
