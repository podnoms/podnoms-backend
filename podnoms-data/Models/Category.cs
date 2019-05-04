using System;
using System.Collections.Generic;

namespace PodNoms.Data.Models {
    public class Category : BaseEntity {
        public string Description { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }
    public class Subcategory : BaseEntity {
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

    }
    // public class PodcastCategory : BaseEntity {
    //     public Guid PodcastId { get; set; }
    //     public Podcast Podcast { get; set; }
    //     public Guid CategoryId { get; set; }
    //     public Category Category { get; set; }
    // }
}
