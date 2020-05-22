using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Collections;
using PodNoms.Common.Utils.Crypt;

namespace PodNoms.Api {
    public class Program {
        private static readonly bool _isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        public static void Main(string[] args) {
            var salt = PBKDFGenerators.GenerateSalt();
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
                              Console.WriteLine($"Bootstrapping prod");

                              config.AddAzureKeyVault(
                                  $"https://{builtConfig["KeyVaultSettings:Vault"]}.vault.azure.net/",
                                  builtConfig["KeyVaultSettings:ClientId"],
                                  builtConfig["KeyVaultSettings:ClientSecret"]);
                          }
                      });

            var t = builder.UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxRequestBodySize = 1073741824;
                    if (_isDevelopment) {

                        var c = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.Development.json", optional: false)
                            .AddEnvironmentVariables("ASPNETCORE_")
                            .Build();

                        var certificate = new X509Certificate2(
                            c[$"DevSettings:CertificateFile{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "")}"],
                            c["DevSettings:CertificateSecret"]);

                        options.Listen(IPAddress.Any, 5001, listenOptions => {
                            listenOptions.UseHttps(certificate);
                        });
                        options.Listen(IPAddress.Any, 5000);
                    }
                });

            return t.Build();
        }
    }
}
