using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Downloaders;
using Xunit;

namespace PodNoms.Tests {
    public class DownloaderTests {
        const string YT_URL = "https://www.youtube.com/watch?v=M2dhD9zR6hk";

        string[] YTDL_URLS = {
            "https://www.mixcloud.com/podnoms/15-minute-sine/",
            "https://soundcloud.com/fergalmoran/isthisit",
        };

        const string AUDIO_URL =
            "https://podnomscdn.blob.core.windows.net/audio/7f06901a-d80e-430d-34c4-08d888d6cd8e.mp3";

        const string PARSEABLE_URL = "https://www.google.com";

        [Fact]
        public async Task Direct_Link_Downloads() {
            var file = await new DirectDownloader()
                .DownloadFromUrl(AUDIO_URL, Path.GetTempFileName(), string.Empty, null);

            Assert.True(!string.IsNullOrEmpty(file) && File.Exists(file));
        }

        [Fact]
        public async Task YouTube_Link_Downloads() {
            var file = await new YouTubeDownloader()
                .DownloadFromUrl(YT_URL, $"{Path.GetTempFileName()}.mp3", string.Empty, null);

            Assert.True(!string.IsNullOrEmpty(file) && File.Exists(file));
        }

        [Fact]
        public async Task YtDl_Link_Downloads() {
            foreach (var url in YTDL_URLS) {
                var file = await new YtDlDownloader()
                    .DownloadFromUrl(url, $"{Path.GetTempFileName()}.mp3", string.Empty, null);

                Assert.True(!string.IsNullOrEmpty(file) && File.Exists(file));
            }
        }

        [Fact]
        public async Task YtDl_InvalidLink_FailDownload() {
            var file = await new YtDlDownloader()
                .DownloadFromUrl(PARSEABLE_URL, $"{Path.GetTempFileName()}.mp3", string.Empty, null);

            Assert.False(!string.IsNullOrEmpty(file) && File.Exists(file));
        }
    }
}
