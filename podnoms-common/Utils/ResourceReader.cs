using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PodNoms.Common.Utils {
    public static class ResourceReader {
        public static async Task<string> ReadResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is null) return string.Empty;

            var resourceStream = assembly.GetManifestResourceStream($"PodNoms.Common.Resources.{resourceName}");
            if (resourceStream != null) {
                string ret;
                using (var reader = new StreamReader(resourceStream, Encoding.UTF8)) {
                    ret = reader.ReadToEnd();
                }

                return ret;
            }

            //MASSIVE HACK here - docker is not reading embedded resource for some reason
            using (var sr = new StreamReader(File.Open($"/app/Resources/{resourceName}", FileMode.Open))) {
                // Read the stream to a string, and write the string to the console.
                return await sr.ReadToEndAsync();
            }
        }
    }
}