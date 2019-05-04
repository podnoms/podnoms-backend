namespace PodNoms.Common.Data.Settings {
    public class StripeSettings {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
    public class PaymentSettings {
        public StripeSettings StripeSettings { get; set; }
    }
}