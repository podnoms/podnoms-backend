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
    public class TestInfoItem {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class DependencySetupFixture {
        public string ROOT_URL = "https://podnoms.blob.core.windows.net/testing/pageparser";

        public List<TestInfoItem> YOUTUBE_URLS = new() {
            new TestInfoItem {Title = "The Countdown Clock", Url = "https://www.youtube.com/watch?v=M2dhD9zR6hk"}
        };

        public List<TestInfoItem> YTDL_URLS = new() {
            new TestInfoItem {Title = "15 Minute Sine", Url = "https://www.mixcloud.com/podnoms/15-minute-sine/"},
            new TestInfoItem
                {Title = "1 Minute Sine", Url = "https://soundcloud.com/fergalmoran/1-minute-sine/s-X7DvUKAezh8"}
        };

        public List<TestInfoItem> AUDIO_URLS = new() {
            new TestInfoItem {
                Title = "7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3",
                Url = "https://podnoms.blob.core.windows.net/audio/7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3"
            }
        };

        public List<TestInfoItem> PLAYLIST_URLS = new() {
            new TestInfoItem {Title = "podnoms", Url = "https://www.mixcloud.com/podnoms/"},
            new TestInfoItem {
                Title = "PODNOMSTEST", Url = "https://www.youtube.com/playlist?list=PLrYP5MfQIpiIipTwZHC_W3QE4iKNSevIB"
            }
        };

        public string PARSEABLE_URL = "https://www.google.com";

        public List<TestInfoItem> TestList {
            get {
                return AUDIO_URLS
                    .Concat(YOUTUBE_URLS.Concat(YTDL_URLS))
                    .ToList();
            }
        }

        public IEnumerable<string> Urls => TestList
            .Select(r => r.Url)
            .ToArray();

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
