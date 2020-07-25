using System.Text.Json.Serialization;

namespace PodNoms.Common.Data.ViewModels.Remote.Google {
    public class YouTubeChannelApiQueryResult {
        /// <summary>Etag of this resource.</summary>
        [JsonPropertyName("etag")]
        public virtual string ETag { get; set; }

        /// <summary>Serialized EventId of the request which produced this response.</summary>
        [JsonPropertyName("eventId")]
        public virtual string EventId { get; set; }

        [JsonPropertyName("items")] public virtual System.Collections.Generic.IList<YouTubeChannel> Items { get; set; }

        /// <summary>Identifies what kind of resource this is. Value: the fixed string
        /// "youtube#channelListResponse".</summary>
        [JsonPropertyName("kind")]
        public virtual string Kind { get; set; }
    }

    public class YouTubeChannel {
        /// <summary>Etag of this resource.</summary>
        [JsonPropertyName("etag")]
        public virtual string ETag { get; set; }

        /// <summary>The ID that YouTube uses to uniquely identify the channel.</summary>
        [JsonPropertyName("id")]
        public virtual string Id { get; set; }
    }
}
