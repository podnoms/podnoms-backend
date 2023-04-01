using System;
using System.IO;

namespace PodNoms.AudioParsing.Helpers;

public static class PathUtils {
    public static string GetScopedTempPath() {
        var path = Path.Combine(Path.GetTempPath(), "podnoms/");
        if (!Path.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public static string GetScopedTempFile(string extension) =>
        $"{Path.Combine(GetScopedTempPath(), $"{Guid.NewGuid()}.{extension.TrimStart('.')}")}";
}
