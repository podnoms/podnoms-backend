using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PodNoms.Common.Services.Storage {
    public abstract class CachedFormFileStorage {
        public static async Task<string> CacheItem(string rootFolder, IFormFile file) {
            var path = Path.Combine(rootFolder, ".ul-cache");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            var fileName = Path.Combine(path, System.Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
            await using var stream = new FileStream(fileName, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }
    }
}
