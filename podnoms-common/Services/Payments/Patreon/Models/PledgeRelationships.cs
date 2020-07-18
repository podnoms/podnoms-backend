using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class PledgeRelationships {
        [JsonProperty(PropertyName = "patron")]
        public User Patron { get; set; }

        [JsonProperty(PropertyName = "reward")]
        public Reward Reward { get; set; }

        [JsonProperty(PropertyName = "creator")]
        public User Creator { get; set; }

        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }
    }
}
