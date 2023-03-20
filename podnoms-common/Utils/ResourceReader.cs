using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PodNoms.Common.Services.Minifier;

namespace PodNoms.Common.Utils {
    public static class ResourceReader {
        public static async Task<string> ReadResource(string resourceName, bool minifyContent = false) {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is null) return string.Empty;

            var resourceStream = assembly.GetManifestResourceStream($"PodNoms.Common.Resources.{resourceName}");
            if (resourceStream != null) {
                using (var reader = new StreamReader(resourceStream, Encoding.UTF8)) {
                    return minifyContent ?
                        reader.MinifyHtmlCode(new Features(new string[] { })) :
                        reader.ReadToEnd();
                }
            }

            //MASSIVE HACK here - docker is not reading embedded resource for some reason
            using (var reader = new StreamReader(File.Open($"/app/Resources/{resourceName}", FileMode.Open))) {
                // Read the stream to a string, and write the string to the console.
                return minifyContent ?
                       reader.MinifyHtmlCode(new Features(new string[] { })) :
                       await reader.ReadToEndAsync();
            }
        }
    }
}
