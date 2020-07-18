using System;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Payments.Patreon.Models {
    public class CampaignAttributes {
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        [JsonProperty(PropertyName = "creation_name")]
        public string CreationName { get; set; }

        [JsonProperty(PropertyName = "pay_per_name")]
        public string PayPerName { get; set; }

        [JsonProperty(PropertyName = "one_liner")]
        public string OneLiner { get; set; }

        [JsonProperty(PropertyName = "main_video_embed")]
        public string MainVideoEmbed { get; set; }

        [JsonProperty(PropertyName = "main_video_url")]
        public string MainVideoUrl { get; set; }

        [JsonProperty(PropertyName = "image_small_url")]
        public string ImageSmallUrl { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "thanks_video_url")]
        public string ThanksVideoUrl { get; set; }

        [JsonProperty(PropertyName = "thanks_embed")]
        public string ThanksEmbed { get; set; }

        [JsonProperty(PropertyName = "thanks_msg")]
        public string ThanksMsg { get; set; }

        [JsonProperty(PropertyName = "is_monthly")]
        public bool IsMonthly { get; set; }

        [JsonProperty(PropertyName = "is_nsfw")]
        public bool IsNSFW { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty(PropertyName = "pledge_url")]
        public string PledgeUrl { get; set; }

        [JsonProperty(PropertyName = "pledge_sum")]
        public int PledgeSum { get; set; }

        [JsonProperty(PropertyName = "patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty(PropertyName = "creation_count")]
        public int CreationCount { get; set; }

        [JsonProperty(PropertyName = "outstanding_payment_amount_cents")]
        public int OutstandingPaymentAmountCents { get; set; }
    }
}
