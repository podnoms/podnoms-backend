using System;

namespace PodNoms.Common.Data.ViewModels {
    public class PaymentLogViewModel {
        public string TransactionId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public string ReceiptURL { get; set; }
    }
}