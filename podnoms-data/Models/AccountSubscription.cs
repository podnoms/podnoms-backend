using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public enum AccountSubscriptionType {
        Advanced,
        Professional
    }
    public class AccountSubscription : IEntity {


        public Guid Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string TransactionId { get; set; }
        public ApplicationUser AppUser { get; set; }
        public AccountSubscriptionType Type { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string ReceiptURL { get; set; }
    }
}