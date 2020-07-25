using System.Collections.Generic;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Settings {
    public class PatreonSettings {
        public string CampaignId { get; set; }
        public string AppName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        // not sure I see these?
        public string CreatorsAccessToken { get; set; }
        public string CreatorsRefreshToken { get; set; }

        public List<PatreonTier> Tiers { get; set; }
    }

    public class PatreonTier {
        public string Id { get; set; }
        public AccountSubscriptionTier Level { get; set; }
    }
}
