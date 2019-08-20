using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PodNoms.Api {
    public class Program {
        private static readonly bool _isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        public static void Main(string[] args) {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) {

            var builder = WebHost.CreateDefaultBuilder(args)
                      .ConfigureAppConfiguration((context, config) => {
                          if (!_isDevelopment) {
                              config.SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: false)
                                  .AddJsonFile("azurekeyvault.json", optional: true, reloadOnChange: true);
                              var builtConfig = config.Build();
                              config.AddAzureKeyVault(
                                  $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                                  builtConfig["KeyVaultSettings:ClientId"],
                                  builtConfig["KeyVaultSettings:ClientSecret"])
                                  //add env vars last so they have highest precedence
                                  //this is useful when debugging prod
                                  .AddEnvironmentVariables("ASPNETCORE_");
                          }
                      });

            var t = builder.UseStartup<Startup>()
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
                        options.Listen(IPAddress.Loopback, 5001, listenOptions => {
                            listenOptions.UseHttps(certificate);
                        });
                    }
                });

            return t.Build();
        }
    }
}
