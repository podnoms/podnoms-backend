using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PodNoms.Common.Services.NYT.Models;

namespace PodNoms.Common.Utils.RemoteParsers {
    public interface IRemoteAudioUrlQueryService {
        RemoteUrlType GetUrlType(string url);
        Task<List<ParsedItemResult>> GetPlaylistItems(string url, DateTime cutoffDate, int count = 10);
        bool ValidateUrl(string url);
    }
    public interface IYouTubeParser : IRemoteAudioUrlQueryService {
        Task<string> GetChannelId(string channelName);
        string GetChannelIdentifier(string url);
        Task<RemoteVideoInfo> GetInformation(string url);
    }
    public interface IMixCloudParser : IRemoteAudioUrlQueryService {

    }
}
