
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using WP = Lib.Net.Http.WebPush;
using WebPush;

namespace PodNoms.Api.Services.Push {
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
        public async Task SendNotificationAsync(WP.PushSubscription subscription, PushMessage message) {
            // var pushEndpoint = "https://fcm.googleapis.com/fcm/send/dAa2ejxsN1E:APA91bH522yYqXlaSSsqGmyELdpf9K3OhbHzakvyhXEKI8fAEEQkrVQSv28fg-VVSyvD-8NqgiLGXm-UI7HcwY-lzsHvYi8hKeu_vbpQ9Xdh7wQP_jTiUDmxwK7TFHeK7_BvfsgXJcssBZb3-McHznazmoXkKVhLcg";
            // var p256dh = "BDCl0Kw2MUckep1GdF9WJhZ1WT7gAe456rmB-nT95N_0925hbCdUmdUGk71eqzf1T_Wf2ohYAPXzVDGM62S4Wfg";
            // var auth = "C0EMaBqxIoxICwvyq9NnHA";

            // var subject = @"mailto:info@podnoms.com";
            // var publicKey = @"BJQY5jNSGoa3SVqxlHH3fyhpBx_7pMrqijh92bM4cwZlmfSYrsRG-8Ci1VYkHr3W13Uh2nWmLTRL00pc7HBdias";
            // var privateKey = @"eFxm_sq3OesJ1ZZUCWCJ0uGqpeWXimOunTqt4CSfwjw";

            // var subscription = new WebPush.PushSubscription(pushEndpoint, p256dh, auth);
            // var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
            // _logger.LogDebug("Sending message!");
            // var payload = JsonConvert.SerializeObject(
            //     new {
            //         notification = new {
            //             title = "PodNoms - AUdio ",
            //             body = "message",
            //             icon = "https://podnoms.com/assets/img/logo-icon.png",
            //             click_action = "https://podnoms.com"
            //         }
            //     });
            // var client = new WebPushClient();
            // try {
            //     client.SendNotification(subscription, payload, vapidDetails);
            // } catch (WebPushException e) {
            //     _logger.LogError($"ERROR: {e.Message}");
            // }

            var sub = new WebPush.PushSubscription(
                    subscription.Endpoint,
                    subscription.Keys["p256dh"],
                    subscription.Keys["auth"]
            );

            var vapid = new WebPush.VapidDetails("mailto: support@podnoms.com", _options.PublicKey, _options.PrivateKey);
            var payload = JsonConvert.SerializeObject(new {
                notification = new {
                    title = message.Topic,
                    body = message.Content,
                    icon = _options.ImageUrl,
                    click_action = message
                }
            });

            var client = new WebPush.WebPushClient();
            try {
                await client.SendNotificationAsync(sub, payload, vapid);
                _logger.LogDebug($"VAPID: Push to {subscription.Endpoint}");
            } catch (WebPushException ex) {
                _logger.LogError("ERROR in VAPID", ex);
            }
        }
    }
}
