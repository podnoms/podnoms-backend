using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PodNoms.Api.Utils {
    public class ImageUtils {
        public static async Task<string> GetRemoteImageAsBase64(string url) {

            var file = await HttpUtils.DownloadFile(url);
            return await ImageAsBase64(file);
        }
        public static async Task<string> ImageAsBase64(string file) {
            if (System.IO.File.Exists(file)) {
                byte[] data = await System.IO.File.ReadAllBytesAsync(file);
                string base64 = System.Convert.ToBase64String(data);
                return $"data:image/jpeg;base64,{base64}";
            }
            return string.Empty;
        }

        public static string GetTemporaryImage(string type, int upperBound, string extension = "png") {
            return $"{type}/image-{Randomisers.RandomInteger(1, upperBound)}.{extension}";
        }
        public static (string, string) ConvertFile(string file, string prefix, string outputType="png") {
            // return (cacheFile, "jpg");
            var outputFile = Path.Combine(Path.GetTempPath(), $"{prefix}.{outputType}");
            if (System.IO.File.Exists(outputFile))
                System.IO.File.Delete(outputFile);

            using (Image<Rgba32> image = Image.Load(file)) {
                image.Mutate(x => x
                    .Resize(1400, 1400));
                using (var outputStream = new FileStream(outputFile, FileMode.CreateNew)) {
                    switch(outputType.ToLower()){
                        case "jpg":
                            image.SaveAsJpeg(outputStream);
                            break;
                        case "gif":
                            image.SaveAsGif(outputStream);
                            break;
                        case "bmp":
                            image.SaveAsBmp(outputStream);
                            break;
                        default:
                            image.SaveAsPng(outputStream);
                            break;
                    }
                }
            }
            return (outputFile, outputType);
        }
        public static string CreateThumbnail(string cacheFile, string prefix, int width, int height, string extension = "png") {
            var outputFile = Path.Combine(Path.GetTempPath(), $"{prefix}-{width}x{height}.png");
            if (System.IO.File.Exists(outputFile))
                System.IO.File.Delete(outputFile);

            using (Image<Rgba32> image = Image.Load(cacheFile)) {
                image.Mutate(x => x
                    .Resize(width, height));
                using (var outputStream = new FileStream(outputFile, FileMode.CreateNew)) {
                    image.SaveAsPng(outputStream);
                }
            }
            return outputFile;
        }
    }
}