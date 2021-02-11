using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;
using Newtonsoft.Json;
using PodNoms.Common.Data.ViewModels.Remote;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PodNoms.Common.Utils {
    public class ImageUtils {
        public static async Task<string> GetRemoteImageAsBase64(string url) {
            var file = await HttpUtils.DownloadFile(url);
            return await ImageAsBase64(file);
        }

        public static async Task<string> ImageAsBase64(string file) {
            if (File.Exists(file)) {
                var data = await File.ReadAllBytesAsync(file);
                var base64 = System.Convert.ToBase64String(data);
                return $"data:image/jpeg;base64,{base64}";
            }

            return string.Empty;
        }

        public static string GetTemporaryImage(string type, int upperBound, string extension = "jpg") {
            return $"{type}/image-{Randomisers.RandomInteger(1, upperBound)}.{extension}";
        }

        public static async Task<(string, string)> ConvertFileFromWebp(string file, string prefix,
            string outputType = "jpg") {
            var outputFile = Path.Combine(Path.GetTempPath(), $"{prefix}.{outputType}");

            if (File.Exists(outputFile))
                File.Delete(outputFile);


            using var image = new MagickImage(file);
            // Save frame as jpg
            await image.WriteAsync(
                new FileStream(outputFile, FileMode.CreateNew),
                MagickFormat.Jpeg
            );
            return (outputFile, outputType);
        }

        public static async Task<(string, string)> ConvertFile(string file, string prefix, string outputType = "jpg") {
            // return (cacheFile, "jpg");
            var outputFile = Path.Combine(Path.GetTempPath(), $"{prefix}.{outputType}");
            if (File.Exists(outputFile))
                File.Delete(outputFile);

            using var image = await Image.LoadAsync(file);

            await using var outputStream = new FileStream(outputFile, FileMode.CreateNew);
            switch (outputType.ToLower()) {
                case "jpg":
                    await image.SaveAsJpegAsync(outputStream);
                    break;
                case "gif":
                    await image.SaveAsGifAsync(outputStream);
                    break;
                case "bmp":
                    await image.SaveAsBmpAsync(outputStream);
                    break;
                default:
                    await image.SaveAsPngAsync(outputStream);
                    break;
            }

            return (outputFile, outputType);
        }

        public static string CreateThumbnail(string cacheFile, string prefix, int width, int height,
            string extension = "png") {
            var outputFile = Path.Combine(Path.GetTempPath(), $"{prefix}-{width}x{height}.png");
            if (File.Exists(outputFile))
                File.Delete(outputFile);

            using (var image = Image.Load(cacheFile)) {
                image.Mutate(x => x
                    .Resize(width, height));
                using (var outputStream = new FileStream(outputFile, FileMode.CreateNew)) {
                    image.SaveAsPng(outputStream);
                }
            }

            return outputFile;
        }

        public static async Task<string> GetRandomImageAsBase64(IHttpClientFactory clientFactory) {
            var client = clientFactory.CreateClient("unsplash");

            var response = await client.GetAsync("/photos/random");
            if (response.IsSuccessStatusCode) {
                var body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(body)) {
                    var imageData = JsonConvert.DeserializeObject<UnsplashViewModel>(body);
                    var base64 = await ImageUtils.GetRemoteImageAsBase64(imageData.urls.regular);
                    return base64;
                }
            }

            return string.Empty;
        }
    }
}
