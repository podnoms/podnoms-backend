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
                        var builtConfig = config.Build();

                        config.AddAzureKeyVault(
                            $"https://{builtConfig["Vault"]}.vault.azure.net/",
                            builtConfig["ClientId"],
                            builtConfig["ClientSecret"]);
                    }
                })
                .UseStartup<JobsStartup>();
    }
}
