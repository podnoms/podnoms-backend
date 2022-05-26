using System;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.AudioParsing.UrlParsers;

public class MixcloudPlaylistParser : IUrlParser {
    private readonly string[] _validPaths = new string[] {
        "stream", "uploads", "favorites", "listens", "playlists"
    };

    public Task<bool> IsMatch(string url) {
        return Task.Factory.StartNew(() => {
            try {
                var uri = new Uri(url);
                if (uri.Host.EndsWith("mixcloud.com")) {
                    var path = uri.Segments[^1].TrimEnd(new[] {'/'});
                    return (_validPaths.Any(path.Equals)) ||
                           uri.Segments.Count(s => s != "/") == 1;
                }
            } catch {
            }

            return false;
        });
    }
}
