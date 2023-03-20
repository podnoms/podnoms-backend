﻿using System;
using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class PodcastEntryViewModel : PodcastEntryShortViewModel {
        public string Description { get; set; }
    }

    public class PodcastEntryShortViewModel {
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Author { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string StrippedDescription { get; set; }
        public List<TagViewModel> Tags { get; set; }
        public string SourceUrl { get; set; }
        public string AudioUrl { get; set; }
        public string PcmUrl { get; set; }
        public float AudioLength { get; set; }
        public long AudioFileSize { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ProcessingStatus { get; set; }

        public bool Processed { get; set; }

        //TODO: Think we can remove this
        public string ProcessingPayload { get; set; }
        public string PodcastId { get; set; }
        public string PodcastSlug { get; set; }
        public string PodcastTitle { get; set; }
        public string UserSlug { get; set; }
        public string UserName { get; set; }
        public string PagesUrl { get; set; }
    }
}
