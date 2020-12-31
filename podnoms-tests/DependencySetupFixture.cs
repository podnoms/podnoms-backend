using System;
using Google.Apis.Http;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.PageParser;

namespace PodNoms.Tests {
    public class DependencySetupFixture {
        public DependencySetupFixture() {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddDbContext<PodNomsDbContext>(options =>
            //     options.UseInMemoryDatabase(databaseName: "TestDatabase"));
            serviceCollection.AddTransient<IPageParser, ExternalPageParser>();

            serviceCollection.AddHttpClient("RemotePageParser", c => {
                c.BaseAddress = new Uri("http://localhost:3000");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            HttpFactory =  serviceCollection.BuildServiceProvider().GetService<IHttpClientFactory>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public IHttpClientFactory? HttpFactory { get; set; }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
