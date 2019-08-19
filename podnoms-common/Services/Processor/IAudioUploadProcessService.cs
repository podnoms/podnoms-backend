using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Processor {
    public interface IAudioUploadProcessService {
        Task<bool> UploadAudio(string userId, Guid entryId, string localFile);
    }
}
