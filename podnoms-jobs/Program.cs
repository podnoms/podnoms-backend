using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PodNoms.Jobs {
    public class Program {
        private static readonly bool _isDevelopment =
      Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development;

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
                .UseStartup<JobsStartup>()
                .UseKestrel(options => {
                    options.Limits.MaxRequestBodySize = 1073741824;
                    if (_isDevelopment && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                        var c = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.Development.json", optional: false)
                              .AddEnvironmentVariables("ASPNETCORE_")
                              .Build();
                        var certificate = new X509Certificate2(
                              c["DevSettings:CertificateFile"],
                              c["DevSettings:CertificateSecret"]);
                        options.Listen(IPAddress.Loopback, 5003, listenOptions => {
                            listenOptions.UseHttps(certificate);
                        });
                    }
                });
    }
}
