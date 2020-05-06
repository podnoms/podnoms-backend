using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using Slack.Webhooks;

namespace PodNoms.Common.Services.Slack {
    public class SlackSupportClient {
        private readonly ChatSettings _chatSettings;

        public SlackSupportClient(IOptions<ChatSettings> options) {
            _chatSettings = options.Value;
        }

        public async Task<bool> NotifyUser(ChatViewModel message) {
            if (string.IsNullOrEmpty(_chatSettings.SlackWebhookUrl))
                return false;

            var slackClient = new SlackClient(_chatSettings.SlackWebhookUrl);
            var slackMessage = new SlackMessage {
                Channel = _chatSettings.SlackChannel,
                Text = message.Message,
                IconEmoji = Emoji.HearNoEvil,
                Username = message.FromUserName
            };
            return await slackClient.PostAsync(slackMessage);
        }
    }
}
