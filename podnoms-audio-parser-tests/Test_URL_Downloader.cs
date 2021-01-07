using System.Threading.Tasks;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.UrlParsers;
using Xunit;

namespace PodNomsAudioParsingService.Tests {
    public class DownloaderTests {
        const string YT_URL = "https://www.youtube.com/watch?v=M2dhD9zR6hk";

        string[] YTDL_URLS = {
            "https://www.mixcloud.com/fergalmoran/dmtf_secs-900_freq-440_amp_08/",
            "https://soundcloud.com/fergalmoran/isthisit",
        };

        const string AUDIO_URL =
            "https://podnomscdn.blob.core.windows.net/audio/7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3";

        const string PARSEABLE_URL = "https://www.google.com";

        [Fact]
        public async Task Direct_Link_Downloads() {
            var file = await new DirectDownloader()
                .DownloadFromUrl(AUDIO_URL, string.Empty);

            Assert.True(!string.IsNullOrEmpty(file) && System.IO.File.Exists(file));
        }

        [Fact]
        public async Task YouTube_Link_Downloads() {
            var file = await new YouTubeDownloader()
                .DownloadFromUrl(YT_URL, string.Empty);

            Assert.True(!string.IsNullOrEmpty(file) && System.IO.File.Exists(file));
        }

        [Fact]
        public async Task YtDl_Link_Downloads() {
            foreach (var url in YTDL_URLS) {
                var file = await new YtDlDownloader()
                    .DownloadFromUrl(url, string.Empty);

                Assert.True(!string.IsNullOrEmpty(file) && System.IO.File.Exists(file));
            }
        }

        [Fact]
        public async Task YtDl_InvalidLink_FailDownload() {
            var file = await new YtDlDownloader()
                .DownloadFromUrl(PARSEABLE_URL, string.Empty);

            Assert.False(!string.IsNullOrEmpty(file) && System.IO.File.Exists(file));
        }
    }
}
