using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Push {
    public class FirebasePushNotificationService : IPushNotificationService {
        private readonly PushNotificationServiceOptions _options;
        private readonly ILogger<FirebasePushNotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public string PublicKey => _options.PublicKey;
        public FirebasePushNotificationService(IOptions<PushNotificationServiceOptions> pushOptions,
                IHttpClientFactory httpClientFactory,
                ILogger<FirebasePushNotificationService> logger) {
            _options = pushOptions.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task SendNotificationAsync(PushSubscription subscription, PushMessage message, string target) {
            _logger.LogInformation($"Sending firebase push: {message.Content}");
            var fb_message = new {
                notification = new {
                    title = message.Topic,
                    body = message.Content,
                    icon = _options.ImageUrl,
                    click_action = target,
                },
                to = subscription.Endpoint
            };
            var data = JsonConvert.SerializeObject(fb_message);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "key",
                $"={_options.PrivateKey}");
            var result = await client.PostAsync(subscription.Endpoint, /*_options.PushUrl, */content);
            _logger.LogInformation("FCM: ", result.Content);
        }
    }
}
