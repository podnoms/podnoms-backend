using EasyNetQ;

namespace PodNoms.Common.Data.Messages {
    [Queue("PodNoms.Client", ExchangeName = "PodNoms.Client")]
    public sealed class RealtimeUpdateMessage {
        public string UserId { get; set; }
        public string ChannelName { get; set; }
        public object Data { get; set; }
    }
}
