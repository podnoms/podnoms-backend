using System.Collections.Generic;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class CampaignRelationships {
        [JsonProperty(PropertyName = "creator")]
        public User Creator { get; set; }

        [JsonProperty(PropertyName = "rewards")]
        public List<Reward> Rewards { get; set; }

        [JsonProperty(PropertyName = "goals")]
        public List<Goal> Goals { get; set; }

        [JsonProperty(PropertyName = "pledges")]
        public List<Pledge> Pledges { get; set; }
    }
}
