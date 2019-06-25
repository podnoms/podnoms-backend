using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class PlaylistViewModel  {
        public string Id { get; set; }        
        public string SourceUrl { get; set; }
        public PodcastViewModel Podcast { get; set; }
        public List<PodcastEntryViewModel> PodcastEntries { get; set; }
    }
}