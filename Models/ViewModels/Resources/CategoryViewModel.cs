using System;
using System.Collections.Generic;

namespace PodNoms.Api.Models.ViewModels {
    public class CategoryViewModel {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public List<SubcategoryViewModel> Children { get; set; }
    }

    public class SubcategoryViewModel {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}