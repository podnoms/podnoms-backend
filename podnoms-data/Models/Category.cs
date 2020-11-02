using System;
using System.Collections.Generic;

namespace PodNoms.Data.Models {
    public class Category : BaseEntity {
        public string Description { get; set; }
        public virtual List<Subcategory> Subcategories { get; set; }
    }

    public class Subcategory : BaseEntity {
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
