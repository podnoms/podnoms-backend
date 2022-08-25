using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.UrlParsers;
using PodNoms.Common.Utils.RemoteParsers;
using Xunit;

namespace PodNoms.Tests {
    public class PlaylistsTests : IClassFixture<DependencySetupFixture> {
        private readonly DependencySetupFixture _fixture;

        public PlaylistsTests(DependencySetupFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test_Mixcloud_Playlist() {
            var parser = _fixture.ServiceProvider.GetRequiredService<MixcloudParser>();

            foreach (var url in _fixture.PLAYLIST_URLS) {
                var entries = await parser.GetEntries(url.Key);
                Assert.Equal(entries.Count, url.Value);
            }
        }
    }
}
