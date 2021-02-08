using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest.Azure;
using PodNoms.Common.Services.PageParser;
using Xunit;
using Xunit.Sdk;

namespace PodNoms.Tests {
    public class ExternalPageParserTests : IClassFixture<DependencySetupFixture> {
        private ServiceProvider _serviceProvider;
        private const string _rootUrl = "https://podnomscdn.blob.core.windows.net/testing/pageparser";

        public ExternalPageParserTests(DependencySetupFixture fixture) {
            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task Test_GetPage_Title() {
            using var scope = _serviceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_rootUrl}/shallow-parser.html");
            var result = await parser.GetPageTitle();
            Assert.Equal("PodNoms Test - Shallow Parser", result);
        }

        [Fact]
        public async Task Test_GetMeta_Tags() {
            using var scope = _serviceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_rootUrl}/shallow-parser.html");
            var result = await parser.GetHeadTags();
            Assert.Contains("og:image", result.Keys);
            Assert.Contains("og:title", result.Keys);
            Assert.Contains("og:url", result.Keys);
            Assert.Contains("META-1-IMAGE", result.Values);
            Assert.Contains("META-2-TITLE", result.Values);
            Assert.Contains("META-3-URL", result.Values);
        }

        [Fact]
        public async Task Test_Audio_Links_Shallow() {
            using var scope = _serviceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_rootUrl}/shallow-parser.html");
            var result = await parser.GetAllAudioLinks();
            Assert.Equal(6, result.Count);

            Assert.Contains("test-1.mp3", result.Values);
            Assert.Contains("test-2.mp3", result.Values);
            Assert.Contains("test-3.mp3", result.Values);
            Assert.Contains("test-4.mp3", result.Values);
            Assert.Contains("test-5.mp3", result.Values);
            Assert.Contains("test-6.mp3", result.Values);

            Assert.Contains($"{_rootUrl}/media/test-1.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-2.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-3.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-4.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-5.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-6.mp3", result.Keys);
        }

        [Fact]
        public async Task Test_Audio_Links_Deep() {
            using var scope = _serviceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_rootUrl}/deep-parser.html");
            var result = await parser.GetAllAudioLinks(true);
            Assert.Equal(6, result.Count);

            Assert.Contains("test-1.mp3", result.Values);
            Assert.Contains("test-2.mp3", result.Values);
            Assert.Contains("test-3.mp3", result.Values);
            Assert.Contains("test-4.mp3", result.Values);
            Assert.Contains("test-5.mp3", result.Values);
            Assert.Contains("test-6.mp3", result.Values);

            Assert.Contains($"{_rootUrl}/media/test-1.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-2.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-3.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-4.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-5.mp3", result.Keys);
            Assert.Contains($"{_rootUrl}/media/test-6.mp3", result.Keys);
        }
    }
}
