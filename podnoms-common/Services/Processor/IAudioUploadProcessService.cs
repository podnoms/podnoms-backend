using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Processor {
    public interface IAudioUploadProcessService {
        Task<bool> UploadAudio(string authToken, Guid entryId, string localFile);
    }
}
