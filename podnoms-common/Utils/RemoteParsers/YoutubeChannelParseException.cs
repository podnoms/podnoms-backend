using System;

namespace PodNoms.Common.Utils.RemoteParsers;

public class YoutubeChannelParseException : Exception {
  public YoutubeChannelParseException(string message) : base(message) {
  }
}

public class PlaylistExpiredException : Exception {
  public PlaylistExpiredException(string message) : base(message) {
  }
}
