using System;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.ViewModels {
    public class PaymentLogViewModel {
        public string Id { get; set; }
        public string TransactionId { get; set; }

        public long Amount { get; set; }
        public bool WasSuccessful { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AccountSubscriptionType Type { get; set; }
        public string ReceiptURL { get; set; }
    }
}