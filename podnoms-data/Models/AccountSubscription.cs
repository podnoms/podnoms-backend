using System;
using System.ComponentModel.DataAnnotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public enum AccountSubscriptionType {
        Stripe,
        Patreon
    }

    public enum AccountSubscriptionTier {
        Freeloader,
        Patron,
        AllAccess,
        VIP
    }
    public class AccountSubscription : IEntity {

        public Guid Id { get; set; }
        public ApplicationUser AppUser { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool WasSuccessful { get; set; }

        public long Amount { get; set; }
        public string TransactionId { get; set; }

        public AccountSubscriptionTier Tier { get; set; }
        public AccountSubscriptionType Type { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string ReceiptURL { get; set; }
    }
}
