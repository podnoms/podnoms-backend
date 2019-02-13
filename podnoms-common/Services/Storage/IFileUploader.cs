using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Storage {
    public interface IFileUploader {
        Task<bool> FileExists(string containerName, string fileName);

        Task<string> UploadFile(string sourceFile, string containerName, string destinationFile,
            string contentType, Action<int, long> progressCallback);
    }
}