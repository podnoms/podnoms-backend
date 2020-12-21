using Newtonsoft.Json;

namespace PodNoms.Data.Models {
    public class ActivityLogPodcastEntry : BaseEntity {
        [JsonIgnore]
        public virtual PodcastEntry PodcastEntry { get; set; }
        public string Referrer { get; set; }
        public string UserAgent { get; set; }
        public string ClientAddress { get; set; }
        public string ExtraInfo { get; set; }

        //TODO: This should probably be refactored to a base class
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
