using System;
using System.ComponentModel.DataAnnotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {

    public class ServerShowcase : BaseEntity {
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        [MaxLength(2000)]
        public string Url { get; set; }

        public int? PodcastCountThreshold { get; set; }
        public int? EntryCountThreshold { get; set; }
        public long? DiskSpaceThreshold { get; set; }
    }
}
