#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Http;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Utils.Extensions;
using Xunit;

namespace PodNoms.Tests {
    public class DependencySetupFixture {
        public string ROOT_URL = "https://podnoms.blob.core.windows.net/testing/pageparser";

        public Dictionary<string, string> YOUTUBE_URLS = new() {
            {"The Countdown Clock", "https://www.youtube.com/watch?v=M2dhD9zR6hk"}
        };


        public Dictionary<string, string> YTDL_URLS = new() {
            {"15 Minute Sine", "https://www.mixcloud.com/podnoms/15-minute-sine/"},
            {"1 Minute Sine", "https://soundcloud.com/fergalmoran/1-minute-sine/s-X7DvUKAezh8"}
        };

        public Dictionary<string, string> AUDIO_URLS = new() {
            {
                "7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3",
                "https://podnoms.blob.core.windows.net/audio/7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3"
            }
        };

        public Dictionary<string, string> PLAYLIST_URLS = new() {
            {
                "podnoms", "https://www.mixcloud.com/podnoms/"
            }
        };

        public string PARSEABLE_URL = "https://www.google.com";

        public Dictionary<string, string> TestList {
            get {
                return AUDIO_URLS
                    .MergeLeft(YOUTUBE_URLS)
                    .MergeLeft(YTDL_URLS);
            }
        }

        public string[] Urls {
            get {
                return TestList
                    .Select(r => r.Value)
                    .ToArray();
            }
        }

        public DependencySetupFixture() {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddDbContext<PodNomsDbContext>(options =>
            //     options.UseInMemoryDatabase(databaseName: "TestDatabase"));
            serviceCollection.AddTransient<IPageParser, ExternalPageParser>();

            serviceCollection.AddHttpClient("RemotePageParser", c => {
                c.BaseAddress = new Uri("http://localhost:3000");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            HttpFactory = serviceCollection.BuildServiceProvider().GetService<IHttpClientFactory>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private IHttpClientFactory? HttpFactory { get; set; }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
