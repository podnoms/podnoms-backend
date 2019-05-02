using System.Net.Http;

namespace PodNoms.Common.Utils.Extensions {
    public static class HttpResponseExtensions {
        public static string ToResponseString(this HttpResponseMessage message) {
            return $"Code: {message.StatusCode}\nMessage: {message.Content}\nError: {message.ReasonPhrase}";
        }
    }
}