namespace PodNoms.Common.Utils.RemoteParsers {
    using System;

    public class YoutubeChannelParseException : Exception {
        public YoutubeChannelParseException(string message) : base(message) {
        }
    }
    public class PlaylistExpiredException : Exception {
        public PlaylistExpiredException(string message) : base(message) {
        }
    }
}
