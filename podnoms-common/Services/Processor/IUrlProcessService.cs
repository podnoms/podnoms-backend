using System;
using System.Threading.Tasks;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public interface IUrlProcessService {

        Task<RemoteUrlStatus> ValidateUrl(string url);
        Task<RemoteUrlType> GetInformation(string entryId);
        Task<RemoteUrlType> GetInformation(PodcastEntry entry);
        Task<bool> DownloadAudio(Guid entryId, string outputFile);
        Task<bool> DownloadAudioV2(string outputId, string url, string outputFile, Func<ProcessingProgress, bool> progressCallback);
    }
}
