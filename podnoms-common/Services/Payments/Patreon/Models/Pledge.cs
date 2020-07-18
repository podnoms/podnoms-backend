using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class Address {
    }
    public class Pledge {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public PledgeAttributes Attributes { get; set; }

        [JsonProperty(PropertyName = "relationships")]
        public PledgeRelationships Relationships { get; set; }
    }
}
