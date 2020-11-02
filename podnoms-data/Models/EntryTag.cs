using System.Collections.Generic;

namespace PodNoms.Data.Models {
    public class EntryTag : BaseEntity {
        public EntryTag(string tagName) {
            this.TagName = tagName;
        }

        public string TagName { get; set; }

        //back relationship
        public virtual ICollection<PodcastEntry> Entries { get; set; } = new List<PodcastEntry>();
    }
}
