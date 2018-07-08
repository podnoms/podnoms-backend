using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace PodNoms.Api.Services.Push {
    public class GCMPushNotificationService : IPushNotificationService {
        private readonly PushNotificationServiceOptions _options;
        private readonly ILogger<GCMPushNotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public string PublicKey => _options.PublicKey;
        public GCMPushNotificationService(IOptions<PushNotificationServiceOptions> pushOptions,
                IHttpClientFactory httpClientFactory,
                ILogger<GCMPushNotificationService> logger) {
            _options = pushOptions.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task SendNotificationAsync(PushSubscription subscription, PushMessage message) {
            var client = _httpClientFactory.CreateClient();

        }
    }
}
