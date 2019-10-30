using Newtonsoft.Json;

namespace PodNoms.Data.Models {
    public class ActivityLogPodcastEntry : BaseEntity {
        [JsonIgnore]
        public PodcastEntry PodcastEntry { get; set; }
        public string ClientAddress { get; set; }
        public string ExtraInfo { get; set; }
    }
}
