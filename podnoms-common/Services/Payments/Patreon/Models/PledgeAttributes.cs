using System;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class PledgeAttributes {
        [JsonProperty(PropertyName = "amount_cents")]
        public int AmountCents { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "declined_since")]
        public DateTime? DeclinedSince { get; set; }

        [JsonProperty(PropertyName = "pledge_cap_cents")]
        public int PledgeCapCents { get; set; }

        [JsonProperty(PropertyName = "patron_pays_fees")]
        public bool PatronPaysFees { get; set; }

        [JsonProperty(PropertyName = "total_historical_amount_cents")]
        public int? TotalHistoricalAmountCents { get; set; }

        [JsonProperty(PropertyName = "is_paused")]
        public bool? IsPaused { get; set; }

        [JsonProperty(PropertyName = "has_shipping_address")]
        public bool? HasShippingAddress { get; set; }
    }
}
