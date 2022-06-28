using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodNoms.Common.Utils.RemoteParsers;

public interface IRemoteAudioUrlQueryService {
    Task<List<ParsedItemResult>> GetEntries(string url, string requesterId, DateTime cutoffDate, int count = 10);
    bool ValidateUrl(string url);
}

public interface IMixCloudParser : IRemoteAudioUrlQueryService {
}
