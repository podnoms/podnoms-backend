using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PodNoms.AudioParsing.Helpers {
    public static class HttpHelper {
        public static async Task<string> DownloadFile(string url, string file = "") {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK) {
                return file;
            }

            using var content = response.Content;
            if (string.IsNullOrEmpty(file))
                file = PathUtils.GetScopedTempFile("tmp");
            var result = await content.ReadAsByteArrayAsync();
            await System.IO.File.WriteAllBytesAsync(file, result);
            return file;
        }
    }
}
