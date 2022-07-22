using System.Collections.Generic;

namespace PodNoms.Common.Utils.RemoteParsers {
    public enum RemoteUrlType {
        Invalid,
        SingleItem,
        Playlist,
        Channel,
        User,
        ParsedLinks
    }

    public class RemoteLinkInfo {
        public string Title { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class RemoteUrlStatus {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public List<RemoteLinkInfo> Links { get; set; }
    }
}
