using Microsoft.Extensions.Options;

namespace PodNoms.Data.Models.Settings {
    public class ChatSettings {
        public string CurrentChatUser { get; set; }
        public string SlackWebhookUrl { get; set; }
        public string SlackChannel { get; set; }
    }
}