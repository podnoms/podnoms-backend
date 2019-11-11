using System;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ActivityLogPodcastEntryViewModel {
        public string IncomingUrl { get; set; }
        public string IncomingHost { get; set; }
        public string PodcastSlug { get; set; }
        public string PodcastTitle { get; set; }
        public DateTime DateAccessed { get; set; }
        public string ClientAddress { get; set; }
        public string ExtraInfo { get; set; }
    }
}
