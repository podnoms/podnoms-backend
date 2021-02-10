using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.PageParser;
using Xunit;
using Xunit.Sdk;

namespace PodNoms.Tests {
    public class ExternalPageParserTests : IClassFixture<DependencySetupFixture> {
        private readonly DependencySetupFixture _fixture;

        public ExternalPageParserTests(DependencySetupFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test_GetPage_Title() {
            using var scope = _fixture.ServiceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_fixture.ROOT_URL}/shallow-parser.html");
            var result = await parser.GetPageTitle();
            Assert.Equal("PodNoms Test - Shallow Parser", result);
        }

        [Fact]
        public async Task Test_GetMeta_Tags() {
            using var scope = _fixture.ServiceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_fixture.ROOT_URL}/shallow-parser.html");
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
            using var scope = _fixture.ServiceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_fixture.ROOT_URL}/shallow-parser.html");
            var result = await parser.GetAllAudioLinks();
            Assert.Equal(6, result.Count);

            Assert.Contains("test-1.mp3", result.Values);
            Assert.Contains("test-2.mp3", result.Values);
            Assert.Contains("test-3.mp3", result.Values);
            Assert.Contains("test-4.mp3", result.Values);
            Assert.Contains("test-5.mp3", result.Values);
            Assert.Contains("test-6.mp3", result.Values);

            Assert.Contains($"{_fixture.ROOT_URL}/media/test-1.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-2.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-3.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-4.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-5.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-6.mp3", result.Keys);
        }

        [Fact]
        public async Task Test_Audio_Links_Deep() {
            using var scope = _fixture.ServiceProvider.CreateScope();
            // Arrange
            var parser = scope.ServiceProvider.GetService<IPageParser>();
            if (parser == null) {
                throw new XunitException("Unable to initialise a page parser.");
            }

            await parser.Initialise($"{_fixture.ROOT_URL}/deep-parser.html");
            var result = await parser.GetAllAudioLinks(true);
            Assert.Equal(6, result.Count);

            Assert.Contains("test-1.mp3", result.Values);
            Assert.Contains("test-2.mp3", result.Values);
            Assert.Contains("test-3.mp3", result.Values);
            Assert.Contains("test-4.mp3", result.Values);
            Assert.Contains("test-5.mp3", result.Values);
            Assert.Contains("test-6.mp3", result.Values);

            Assert.Contains($"{_fixture.ROOT_URL}/media/test-1.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-2.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-3.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-4.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-5.mp3", result.Keys);
            Assert.Contains($"{_fixture.ROOT_URL}/media/test-6.mp3", result.Keys);
        }
    }
}
