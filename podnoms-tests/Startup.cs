using System;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Tests.Mocks;

namespace PodNoms.Tests {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddTransient<IYouTubeParser, YouTubeParser>();
            services.AddTransient<IApiKeyRepository, TestApiKeyRepository>();
            services.AddTransient<IExternalServiceRequestLogger, TestExternalServiceRequestLogger>();
            // services.AddTransient<IDownloader, YtDlDownloader>();
            services.AddHttpClient("youtube", c => {
                c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
                c.DefaultRequestHeaders.Add(
                    "Accept",
                    "*/*"
                );
            });
        }
    }
}
