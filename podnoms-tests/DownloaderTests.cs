using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.UrlParsers;
using Xunit;

namespace PodNoms.Tests {
    public class DownloaderTests : IClassFixture<DependencySetupFixture> {
        private DependencySetupFixture _fixture;

        public DownloaderTests(DependencySetupFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task TestDownloaderControlled_Downloads() {
            foreach (var url in _fixture.Urls) {
                var downloader = await new UrlTypeParser().GetDownloader(url);
                var file = await downloader.DownloadFromUrl(
                    url,
                    PathUtils.GetScopedTempFile("mp3"));
                var condition = !string.IsNullOrEmpty(file) && File.Exists(file);
                Assert.True(condition);
            }
        }

        [Fact]
        public async Task YtDl_InvalidLink_FailDownload() {
            var file = await new YtDlDownloader()
                .DownloadFromUrl(
                    _fixture.PARSEABLE_URL,
                    PathUtils.GetScopedTempFile("mp3"));

            Assert.False(!string.IsNullOrEmpty(file) && File.Exists(file));
        }
    }
}
