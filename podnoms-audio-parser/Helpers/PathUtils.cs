using System;
using System.IO;

namespace PodNoms.AudioParsing.Helpers;

public static class PathUtils {
    public static string GetScopedTempPath() => Path.Combine(Path.GetTempPath(), "podnoms/");

    public static string GetScopedTempFile(string extension = "mp3") =>
        $"{Path.Combine(GetScopedTempPath(), $"{Guid.NewGuid()}.{extension.TrimStart('.')}")}";
}
