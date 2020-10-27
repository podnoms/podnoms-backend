namespace PodNoms.Data.Models {
    public class ServicesApiKeyLog : BaseEntity {
        public ServicesApiKeyLog() {
        }

        public ServicesApiKeyLog(ServicesApiKey apiKey, string requesterId, string stackTrace) {
            this.ApiKey = apiKey;
            this.RequesterId = requesterId;
            this.Stack = stackTrace;
        }

        public ServicesApiKey ApiKey { get; set; }
        public string RequesterId { get; set; }
        public string Stack { get; set; }
    }
}
