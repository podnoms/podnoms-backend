namespace PodNoms.Common.Services.Payments {
    public class StripePaymentResult {
        public string Id { get; set; }
        public string Status { get; set; }
        public bool Paid { get; set; }
        public long Amount { get; set; }
        public string ReceiptUrl { get; set; }
    }
}