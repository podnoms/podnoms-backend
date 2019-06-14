using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PodNoms.Api {
    public class Program {
        private static readonly bool _isDevelopment =
            Environment.GetEnvironmentVariable ("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development;

        public static void Main (string[] args) {
            Console.WriteLine (Environment.GetEnvironmentVariable ("ASPNETCORE_ENVIRONMENT"));
            BuildWebHost (args).Run ();
        }

        private static IWebHost BuildWebHost (string[] args) {
            var builder = WebHost.CreateDefaultBuilder (args)
                .ConfigureAppConfiguration ((context, config) => {
                    if (_isDevelopment) return;
                    config.SetBasePath (Directory.GetCurrentDirectory ())
                        .AddJsonFile ("appsettings.json", optional : false)
                        .AddEnvironmentVariables ("ASPNETCORE_");
                    var builtConfig = config.Build ();
                    config.AddAzureKeyVault (
                        $"https://{builtConfig["Vault"]}.vault.azure.net/",
                        builtConfig["ClientId"],
                        builtConfig["ClientSecret"]);
                });
            if (!_isDevelopment) {
                builder.UseApplicationInsights ();
            }

            var t = builder.UseStartup<Startup> ()
                .UseKestrel (options => { options.Limits.MaxRequestBodySize = 1073741824; });
            if (_isDevelopment)
                t = t.UseUrls ("http://*:5000", "https://*:5001");
            return t.Build ();
        }
    }
}
