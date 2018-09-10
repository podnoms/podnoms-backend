using System.Threading.Tasks;

namespace PodNoms.Api.Services.Storage {
    public interface IFileUtilities {
        Task<long> GetRemoteFileSize(string containerName, string fileName);
        Task<bool> CheckFileExists(string containerName, string fileName);
        Task<bool> CopyRemoteFile(string sourceContainer, string sourceFile, string destinationContainer, string destinationFile);
    }
}