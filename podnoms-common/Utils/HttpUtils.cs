﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;

namespace PodNoms.Common.Utils {
    public static class HttpUtils {
        public static string UrlCombine(params string[] parts) => Flurl.Url.Combine(parts);

        public static async Task<string> DownloadText(string url, string contentType = "text/plain") {
            using var client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(contentType));
            var data = await client.GetStringAsync(url);
            return data;
        }

        public static async Task<string> DownloadFile(string url, string file = "") {

            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK) {
                using var content = response.Content;
                if (string.IsNullOrEmpty(file))
                    file = System.IO.Path.GetTempFileName();
                var result = await content.ReadAsByteArrayAsync();
                System.IO.File.WriteAllBytes(file, result);
            }
            return file;
        }

        internal static HttpClientHandler GetFiddlerProxy() {
            var handler = new HttpClientHandler {
                Proxy = new WebProxy("localhost", 8888),
                UseProxy = true
            };
            return handler;
        }

        public static async Task<string> GetUrlExtension(string url) {
            using (var client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await client.SendAsync(request);
                // var response = await client.GetAsync (url, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == HttpStatusCode.OK &&
                    response.Content.Headers.ContentType != null) {
                    var extension = MimeTypeMap.GetExtension(response.Content.Headers.ContentType.MediaType);
                    if (!string.IsNullOrEmpty(extension))
                        return extension.TrimStart('.');
                }
            }
            return string.Empty;
        }
    }
}
