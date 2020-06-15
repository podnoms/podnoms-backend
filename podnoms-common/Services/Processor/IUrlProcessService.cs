using System;
using System.Threading.Tasks;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public interface IUrlProcessService {

        Task<RemoteUrlStatus> ValidateUrl(string url);
        Task<RemoteUrlType> GetInformation(string entryId);
        Task<RemoteUrlType> GetInformation(PodcastEntry entry);
        Task<bool> DownloadAudio(string authToken, Guid entryId, string outputFile);
    }
}
