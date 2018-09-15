using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Processor {
    public interface IAudioUploadProcessService {
        Task<bool> UploadAudio(Guid entryId, string localFile);
    }
}