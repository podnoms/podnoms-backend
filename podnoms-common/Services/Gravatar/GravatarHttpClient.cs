using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Utils.Crypt;

namespace PodNoms.Common.Services.Gravatar {
    public class GravatarHttpClient {
        private readonly HttpClient _client;
        private readonly ILogger<GravatarHttpClient> _logger;

        public GravatarHttpClient(HttpClient client, ILogger<GravatarHttpClient> logger) {
            _client = client;
            _client.BaseAddress = new Uri("https://www.gravatar.com");
            _logger = logger;
        }

        public async Task<string> GetGravatarImage(string email) {
            try {
                var hash = MD5Generator.CalculateMD5Hash(email.ToLower().Trim());
                var gravatarUri = new Uri($"/avatar/{hash}?d=404", UriKind.Relative);
                var res = await _client.GetAsync(gravatarUri);
                res.EnsureSuccessStatusCode();
                return new Uri(_client.BaseAddress, $"/avatar/{hash}").AbsoluteUri;
            }
            catch (Exception ex) {
                _logger.LogError($"Error obtaining gravatar image\n\t{ex.Message}");
            }

            return string.Empty;
        }
    }
}