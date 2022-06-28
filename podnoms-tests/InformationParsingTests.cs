using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.UrlParsers;
using PodNoms.Common.Utils.RemoteParsers;
using Xunit;

namespace PodNoms.Tests {
    public class InformationParsingTests : IClassFixture<DependencySetupFixture> {
        private readonly DependencySetupFixture _fixture;

        public InformationParsingTests(DependencySetupFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test_Links_HaveInfo() {
            foreach (var item in _fixture.TestList) {
                var downloader = await new UrlTypeParser().GetDownloader(item.Url);
                var info = await downloader.GetVideoInformation(item.Url);
                Assert.Equal(item.Title, info.Title);
            }
        }

        [Fact]
        public async Task YtDl_Link_Info() {
            var ytdlInfo = new YtDlDownloader();

            foreach (var item in _fixture.YTDL_URLS) {
                var info = await ytdlInfo.GetVideoInformation(item.Url);
                Assert.Equal(item.Title, info.Title);
            }
        }
    }
}
