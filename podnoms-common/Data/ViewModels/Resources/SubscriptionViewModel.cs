namespace PodNoms.Common.Data.ViewModels.Resources {
    public class SubscriptionViewModel {
        public string UserId { get; set; }
        public bool HasSubscribed { get; set; }
        public string SubscriptionType { get; set; }
        public bool SubscriptionValid { get; set; }
    }
}
