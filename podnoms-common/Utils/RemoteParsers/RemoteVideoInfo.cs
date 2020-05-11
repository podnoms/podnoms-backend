using System;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class RemoteVideoInfo {
        public string VideoId { get; set; }
        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public string Thumbnail { get; internal set; }
        public string Uploader { get; internal set; }
        public DateTime? UploadDate { get; internal set; }
    }
}
