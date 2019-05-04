using System.Threading.Tasks;

namespace PodNoms.Common.Services.Storage {
    public interface IFileUtilities {
        Task<long> GetRemoteFileSize(string containerName, string fileName);
        Task<bool> CheckFileExists(string containerName, string fileName);
        Task<bool> CopyRemoteFile(string sourceContainer, string sourceFile, string destinationContainer, string destinationFile);
    }
}