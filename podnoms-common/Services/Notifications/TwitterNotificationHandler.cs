using System;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Services.Notifications {
    public class TwitterNotificationHandler : BaseNotificationHandler {
        public override Notification.NotificationType Type => Notification.NotificationType.Twitter;

        public TwitterNotificationHandler(INotificationRepository notificationRepository, IHttpClientFactory httpClient)
            : base(notificationRepository, httpClient) { }

        public override async Task<string>
            SendNotification(Guid notificationId, string userName, string title, string message, string url) {
            var config = await _getConfiguration(notificationId);
            if (config is null || !(config.ContainsKey("ConsumerKey") && config.ContainsKey("ConsumerSecret") &&
                                    config.ContainsKey("AccessToken") && config.ContainsKey("AccessTokenSecret")))
                return "Invalid configuration";

            var auth = Tweetinvi.Auth.CreateCredentials(
                config["ConsumerKey"],
                config["ConsumerSecret"],
                config["AccessToken"],
                config["AccessTokenSecret"]);
            var user = Tweetinvi.User.GetAuthenticatedUser(auth);
            var tweet = user.PublishTweet($"New podcast episode - {title}\n{message}\n{url}");

            return $"Tweet Id: {tweet.Id.ToString()}";
        }
    }
}
