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
    public class AccountSubscription : BaseEntity {

        public ApplicationUser AppUser { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool WasSuccessful { get; set; }

        public long Amount { get; set; }
        public string TransactionId { get; set; }

        public AccountSubscriptionTier Tier { get; set; }
        public AccountSubscriptionType Type { get; set; }

        public string ReceiptURL { get; set; }
    }
}
