using EasyNetQ;

namespace PodNoms.Common.Data.Messages {
    [Queue("PodNoms.Client", ExchangeName = "PodNoms.Client")]
    public sealed class PingMessage {
        public string Pong { get; set; }
    }
}
