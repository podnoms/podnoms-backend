using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PodNoms.Data.Models {
    public class PodcastEntrySharingLink : BaseEntity {
        // used for ID generation
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LinkIndex { get; set; }
        public string LinkId { get; set; }
        public virtual PodcastEntry PodcastEntry { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
