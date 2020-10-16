using System.Threading.Tasks;

namespace PodNoms.Common.Utils.RemoteParsers {
    public interface IYouTubeParser : IRemoteAudioUrlQueryService {
        /// <summary>
        /// Retrieve the id part of a video given a URL
        /// Helps to clean up messy URLs that might be thrown at us
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<string> GetVideoId(string url);
        Task<string> GetPlaylistId(string url);
        Task<string> ConvertUserToChannel(string url);
        Task<RemoteVideoInfo> GetVideoInformation(string url);
    }
}
