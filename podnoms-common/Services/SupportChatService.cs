using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Services.Hubs;
using PodNoms.Common.Services.Push;
using PodNoms.Common.Services.Slack;
using PodNoms.Data.Models;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services {
    public class SupportChatService : ISupportChatService {
        private readonly ChatSettings _chatSettings;
        private readonly IPushNotificationService _notificationService;
        private readonly HubLifetimeManager<ChatHub> _hub;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly SlackSupportClient _slackSupport;
        public SupportChatService(UserManager<ApplicationUser> userManager, IOptions<ChatSettings> chatSettings,
                         IPushSubscriptionStore subscriptionStore, IPushNotificationService notificationService,
                         HubLifetimeManager<ChatHub> hub, SlackSupportClient slackSupport) {
            _chatSettings = chatSettings.Value;
            _notificationService = notificationService;
            _hub = hub;
            _userManager = userManager;
            _subscriptionStore = subscriptionStore;
            _slackSupport = slackSupport;

        }
        public async Task<bool> InitiateSupportRequest(string fromUser, ChatViewModel message) {
            if (!string.IsNullOrEmpty(_chatSettings.CurrentChatUser)) {
                var user = await _userManager.FindByEmailAsync(_chatSettings.CurrentChatUser);
                if (!string.IsNullOrEmpty(user?.Id)) {
                    message.ToUserId = user.Id;
                    message.ToUserName = user.FullName;
                    //send firebase message to notify via web worker
                    var pushMessage = new PushMessage(message.Message) {
                        Topic = "New support chat message",
                        Urgency = PushMessageUrgency.Normal
                    };
                    await _subscriptionStore.ForEachSubscriptionAsync(user.Id, (PushSubscription subscription) => {
                        _notificationService.SendNotificationAsync(subscription, pushMessage, string.Empty);
                    });

                    //send SignalR message to notify in chat.component
                    await _hub.SendUserAsync(user.Id, "SendMessage", new object[] { message });

                    //send slack message
                    var slackResult = await _slackSupport.NotifyUser(message);
                    return true;
                }
            }
            return false;
        }
    }
}