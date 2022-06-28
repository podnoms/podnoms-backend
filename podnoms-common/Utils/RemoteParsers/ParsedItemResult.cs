using System;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class ParsedItemResult {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ItemType { get; set; }
        public string Url { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
