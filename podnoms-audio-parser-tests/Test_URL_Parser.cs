using System.Threading.Tasks;
using PodNoms.AudioParsing.UrlParsers;
using Xunit;

namespace PodNomsAudioParsingService.Tests {
    public class ParserTests {
        const string YT_URL = "https://www.youtube.com/watch?v=M2dhD9zR6hk";

        string[] YTDL_URLS = {
            "https://www.mixcloud.com/fergalmoran/2013-birthday-classics/",
            "https://soundcloud.com/fergalmoran/isthisit",
        };

        const string AUDIO_URL =
            "https://podnomscdn.blob.core.windows.net/audio/7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3";

        const string PARSEABLE_URL = "https://www.google.com";

        [Fact]
        public async Task YouTube_Link_Parses() {
            var urlType = await new UrlTypeParser().GetUrlType(YT_URL);
            Assert.Equal(UrlType.YouTube, urlType);
        }

        [Fact]
        public async Task Audio_Link_Parses() {
            var urlType = await new UrlTypeParser().GetUrlType(AUDIO_URL);
            Assert.Equal(UrlType.Direct, urlType);
        }

        [Fact]
        public async Task YtDl_Link_Parses() {
            foreach (var url in YTDL_URLS) {
                var urlType = await new UrlTypeParser().GetUrlType(url);
                Assert.Equal(UrlType.YtDl, urlType);
            }
        }

        [Fact]
        public async Task Parseable_Link_Parses() {
            //this test will only pass if the YtDl test fails
            var urlType = await new UrlTypeParser().GetUrlType(PARSEABLE_URL);
            Assert.Equal(UrlType.PageParser, urlType);
        }
    }
}
