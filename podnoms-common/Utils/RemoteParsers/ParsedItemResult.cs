using System;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class ParsedItemResult : Object {
        public override string ToString() {
            return $"{Title} - {UploadDate.ToString()}";
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string VideoType { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
