using EasyNetQ;

namespace PodNoms.Common.Data.Messages;

[Queue("PodNoms.Client", ExchangeName = "PodNoms.Client")]
public sealed class RealtimeUpdateMessage {
  public string UserId { get; set; }
  public string ChannelName { get; set; }
  public string Title { get; set; }
  public string Message { get; set; }
  public string ImageUrl { get; set; }
}
