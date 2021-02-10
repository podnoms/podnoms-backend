using EasyNetQ;

namespace PodNoms.Common.Data.Messages {
    [Queue("PodNoms.Client", ExchangeName = "PodNoms.Client")]
    public sealed class NotifyUserMessage {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Target { get; set; }
        public string Image { get; set; }
    }
}
