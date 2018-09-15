using System;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class TwitterNotificationHandler : BaseNotificationHandler, INotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.Twitter;

        public TwitterNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<bool> SendNotification(Guid notificationId, string title, string message) {
            var config = await _getConfiguration(notificationId);
            if (config == null || !(config.ContainsKey("ConsumerKey") && config.ContainsKey("ConsumerSecret") &&
                                    config.ContainsKey("AccessToken") && config.ContainsKey("AccessTokenSecret")))
                return false;

            var auth = Tweetinvi.Auth.CreateCredentials(
                config["ConsumerKey"],
                config["ConsumerSecret"],
                config["AccessToken"],
                config["AccessTokenSecret"]);
            var user = Tweetinvi.User.GetAuthenticatedUser(auth);
            var tweet = user.PublishTweet($"New podcast episide - {title}\n{message}");

            return !string.IsNullOrEmpty(tweet.Id.ToString());
        }
    }
}