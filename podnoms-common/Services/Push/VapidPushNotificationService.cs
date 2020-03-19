using System.Net.Http;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebPush;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Push {
    public class VapidPushNotificationService : IPushNotificationService {
        private readonly PushNotificationServiceOptions _options;
        private readonly ILogger<VapidPushNotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public string PublicKey => _options.PublicKey;
        public VapidPushNotificationService(IOptions<PushNotificationServiceOptions> pushOptions,
                IHttpClientFactory httpClientFactory,
                ILogger<VapidPushNotificationService> logger) {
            _options = pushOptions.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task SendNotificationAsync(WP.PushSubscription subscription, PushMessage message, string target) {
            _logger.LogInformation($"Sending VAPID push: {message.Content}: Image {_options.ImageUrl}");
            var sub = new WebPush.PushSubscription(
                    subscription.Endpoint,
                    subscription.Keys["p256dh"],
                    subscription.Keys["auth"]
            );

            var vapid = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
            var payload = JsonConvert.SerializeObject(new {
                notification = new {
                    title = message.Topic,
                    body = message.Content,
                    icon = _options.ImageUrl,
                    click_action = string.IsNullOrEmpty(target) ? _options.ClickUrl : target
                }
            });

            var client = new WebPushClient();
            try {
                _logger.LogDebug($"VAPID: Push to {subscription.Endpoint}");
                await client.SendNotificationAsync(sub, payload, vapid);
            } catch (WebPushException ex) {
                _logger.LogError($"ERROR in VAPID: {ex.Message}");
                _logger.LogError($"{subscription.Endpoint}");
            }
        }
    }
}
