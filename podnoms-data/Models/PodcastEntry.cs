using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PodNoms.Data.Models {
    public enum ShareOptions {
        Public = (1 << 0),
        Private = (1 << 1),
        Download = (1 << 2)
    }
    public enum ProcessingStatus {
        Accepted, //0
        Downloading, //1
        Processing, //2
        Uploading, //3
        Processed, //4
        Failed, //5
        Deferred //6
    }
    public class PodcastEntry : BaseEntity {

        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceUrl { get; set; }
        public string AudioUrl { get; set; }
        public float AudioLength { get; set; }
        public long AudioFileSize { get; set; }
        public string ImageUrl { get; set; }
        public string ProcessingPayload { get; set; }
        public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Accepted;
        public int ShareOptions { get; set; }
        public bool Processed { get; set; }
        public Guid PodcastId { get; set; }

        [JsonIgnore]
        public Podcast Podcast { get; set; }

        [JsonIgnore]
        public List<PodcastEntrySharingLink> SharingLinks { get; set; }
        public string GetThumbnailUrl (string cdnUrl, string containerName) {
            var url = ImageUrl.StartsWith ("http") ?
                ImageUrl :
                $"{cdnUrl}{containerName}/entry/cached/{Id.ToString()}-32x32.png";
            return url;
        }
    }
}
