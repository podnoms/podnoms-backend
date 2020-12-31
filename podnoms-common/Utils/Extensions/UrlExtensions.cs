using System;

namespace PodNoms.Common.Utils.Extensions {
    public static class UrlExtensions {
        public static string GetFilenameFromUrl(this string value) {
            var uri = new Uri(value);
            return uri.Segments[^1];
        }
    }
}
