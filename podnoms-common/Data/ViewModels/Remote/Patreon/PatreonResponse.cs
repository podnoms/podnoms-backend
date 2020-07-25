using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PodNoms.Common.Data.ViewModels.Remote.Patreon {

    public class Datum {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Memberships {
        public List<Datum> data { get; set; }
    }

    public class SubscriberData {
        public string id { get; set; }
        public Relationships relationships { get; set; }
        public string type { get; set; }
    }

    public class CurrentlyEntitledTiers {
        [JsonPropertyName("data")]
        public List<Datum> Tiers { get; set; }
    }

    public class Relationships {
        [JsonPropertyName("currently_entitled_tiers")]
        public CurrentlyEntitledTiers CurrentlyEntitledTiers { get; set; }
    }

    public class SubscriberSubscriptionData {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("relationships")]
        public Relationships RelationShips { get; set; }
    }

    public class Links {
        public string self { get; set; }
    }

    public class PatreonResponse {

        [JsonPropertyName("data")]
        public SubscriberData SubscriberData { get; set; }

        [JsonPropertyName("included")]
        public List<SubscriberSubscriptionData> Subscriptions { get; set; }

        [JsonPropertyName("links")]
        public Links Links { get; set; }
    }
}
