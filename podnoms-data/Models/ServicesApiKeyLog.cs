namespace PodNoms.Data.Models;

public class ServicesApiKeyLog : BaseEntity {
  public ServicesApiKeyLog() {
  }

  public ServicesApiKeyLog(ServiceApiKey apiKey, string requesterId, string stackTrace) {
    ApiKey = apiKey;
    RequesterId = requesterId;
    Stack = stackTrace;
  }

  public virtual ServiceApiKey ApiKey { get; set; }
  public string RequesterId { get; set; }
  public string Stack { get; set; }
}
