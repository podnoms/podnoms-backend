using System.Threading.Tasks;
using PodNoms.AudioParsing.UrlParsers;
using Xunit;

namespace PodNoms.Tests {
    public class ParserTests : IClassFixture<DependencySetupFixture> {
        private readonly DependencySetupFixture _fixture;

        public ParserTests(DependencySetupFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task YouTube_Link_Parses() {
            foreach (var (title, url) in _fixture.YOUTUBE_URLS) {
                var urlType = await new UrlTypeParser().GetUrlType(url);
                Assert.Equal(UrlType.YouTube, urlType);
            }
        }

        [Fact]
        public async Task Audio_Link_Parses() {
            foreach (var (title, url) in _fixture.AUDIO_URLS) {
                var urlType = await new UrlTypeParser().GetUrlType(url);
                Assert.Equal(UrlType.Direct, urlType);
            }
        }

        [Fact]
        public async Task YtDl_Link_Parses() {
            foreach (var (title, url) in _fixture.YTDL_URLS) {
                var urlType = await new UrlTypeParser().GetUrlType(url);
                Assert.Equal(UrlType.YtDl, urlType);
            }
        }        
        [Fact]
        public async Task Playlist_Link_Parses() {
            foreach (var (title, url) in _fixture.PLAYLIST_URLS) {
                var urlType = await new UrlTypeParser().GetUrlType(url);
                Assert.Equal(UrlType.Playlist, urlType);
            }
        }

        [Fact]
        public async Task Parseable_Link_Parses() {
            //this test will only pass if the YtDl test fails
            var urlType = await new UrlTypeParser().GetUrlType(_fixture.PARSEABLE_URL);
            Assert.Equal(UrlType.PageParser, urlType);
        }
    }
}
