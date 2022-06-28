using System.Threading.Tasks;

namespace PodNoms.Common.Utils.RemoteParsers; 

public interface IYouTubeParser : IRemoteAudioUrlQueryService {
    Task<string> GetVideoId(string url, string requesterId);
    Task<string> GetPlaylistId(string url, string requesterId);
    Task<string> ConvertUserToChannel(string url, string requesterId);
    Task<RemoteVideoInfo> GetVideoInformation(string url, string requesterId);
}
