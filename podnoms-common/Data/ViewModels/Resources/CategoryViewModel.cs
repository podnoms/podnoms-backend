using System;
using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class UserActivityViewModel {
        public UserActivityViewModel () { }

        public string Name { get; internal set; }
        public string Slug { get; internal set; }
        public string Email { get; internal set; }
        public List<PodcastViewModel> Podcasts { get; internal set; }

        public int PodcastCount { get; set; }
        public int EntryCount { get; set; }
        public bool IsAdmin { get; set; }
    }
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
