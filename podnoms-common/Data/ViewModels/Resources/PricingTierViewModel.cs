using System.Collections.Generic;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.ViewModels.Resources;

public class PricingTierViewModel {
  public AccountSubscriptionTier Type { get; set; }
  public string Title { get; set; }
  public long CostPerMonth { get; set; }
  public List<string> Details { get; set; }
}

public class PricingTierController {
  public PricingTierController() {
    PricingTiers = new List<PricingTierViewModel>();
    PricingTiers.Add(new PricingTierViewModel {
      Type = AccountSubscriptionTier.Freeloader,
      Title = "Free",
      CostPerMonth = 0,
      Details = new List<string>(new[] {
        "Unlimited Podcasts",
        "1Gb storage",
        "10 episodes per Podcast",
        "rss.podnoms.com domain",
        "Email support"
      })
    });
    PricingTiers.Add(new PricingTierViewModel {
      Type = AccountSubscriptionTier.Patron,
      Title = "Personal",
      CostPerMonth = 499,
      Details = new List<string>(new[] {
        "Unlimited Podcasts",
        "100Gb storage",
        "Unlimited episodes per Podcast",
        "rss.podnoms.com domain",
        "Priority email support"
      })
    });
    PricingTiers.Add(new PricingTierViewModel {
      Type = AccountSubscriptionTier.AllAccess,
      Title = "Professional",
      CostPerMonth = 999,
      Details = new List<string>(new[] {
        "Unlimited Podcasts",
        "Unlimited storage",
        "Unlimited episodes per Podcast",
        "Custom domains",
        "Telephone support"
      })
    });
  }

  public List<PricingTierViewModel> PricingTiers { get; set; }
}
