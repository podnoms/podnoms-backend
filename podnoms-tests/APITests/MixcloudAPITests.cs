using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Utils.RemoteParsers;
using Xunit;

namespace PodNoms.Tests.APITests;

public class MixcloudAPITests : IClassFixture<DependencySetupFixture> {
    private readonly DependencySetupFixture _fixture;

    public MixcloudAPITests(DependencySetupFixture fixture) {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_FullPlaylistReturned() {
        var parser = _fixture.ServiceProvider.GetRequiredService<MixcloudParser>();
        var results = await parser.GetAllEntries("https://api.mixcloud.com/radiootherway");

        Assert.True(results.Count >= 265);
    }
}
