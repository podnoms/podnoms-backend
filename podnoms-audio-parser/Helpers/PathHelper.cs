using System;

namespace PodNoms.AudioParsing.Helpers {
    public static class PathHelper {
        public static string GetTempFileNameWithExtension(string extension = ".tmp") {
            return System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + extension;
        }
    }
}
