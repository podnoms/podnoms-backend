using System.Threading.Tasks;

namespace PodNoms.Common.Utils.RemoteParsers {
    public interface IYouTubeParser : IRemoteAudioUrlQueryService {
        Task<string> GetVideoId(string url);
        Task<string> GetChannelId(string channelName);
        Task<string> GetChannelIdentifier(string url);
        Task<string> ConvertUserToChannel(string url);
        Task<RemoteVideoInfo> GetInformation(string url);
    }
}
