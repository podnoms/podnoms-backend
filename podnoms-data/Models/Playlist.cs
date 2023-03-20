using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PodNoms.Data.Models {
    public class Playlist : BaseEntity {
        //TODO: Update this to use concrete model
        [MaxLength(2000)] //https://stackoverflow.com/questions/417142/what-is-the-maximum-length-of-a-url-in-different-browsers 
        public string SourceUrl { get; set; }

        public Guid PodcastId { get; set; }
        public virtual Podcast Podcast { get; set; }
        public virtual List<PodcastEntry> PodcastEntries { get; set; }
    }
}
