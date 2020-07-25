using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PodNoms.Data.Models {
    public class Playlist : BaseEntity {
        //TODO: Update this to use concrete model
        public string SourceUrl { get; set; }
        public Guid PodcastId { get; set; }
        public Podcast Podcast { get; set; }
        public virtual List<PodcastEntry> PodcastEntries { get; set; }
    }
}
