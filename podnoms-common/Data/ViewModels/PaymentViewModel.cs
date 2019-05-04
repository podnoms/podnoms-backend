using PodNoms.Data.Models;

namespace PodNoms.Common.Data.ViewModels {
    public class PaymentViewModel {
        public string Token { get; set; }
        public AccountSubscriptionType Type { get; set; }
        public long Amount { get; set; }
    }
}