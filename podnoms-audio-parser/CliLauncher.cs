using System;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.AudioParsing.Options;
using PodNoms.AudioParsing.UrlParsers;

namespace PodNoms.AudioParsing {
    public class CliLauncher {
        public static async Task Main(string[] args) {
            Parser.Default.ParseArguments<CliArgs>(args)
                .WithParsed<CliArgs>(async o => {
                    Console.WriteLine($"URL: {o.Url}");
                    switch (o.Action) {
                        case "validate":
                            await ValidateUrl(o.Url, o.CallbackUrl);
                            break;
                        case "process":
                            await ProcessUrl(o.Url, o.CallbackUrl);
                            break;
                    }
                });
        }

        private static async Task<bool> ValidateUrl(string url, string callbackUrl) {
            var urlParser = new UrlTypeParser();
            var urlType = await urlParser.GetUrlType(url);

            return urlType != UrlType.Invalid;
        }

        private static async Task<bool> ProcessUrl(string url, string callbackUrl) {
            //get the type of the URL
            var urlParser = new UrlTypeParser();
            var urlType = await urlParser.GetUrlType(url);
            string outputFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.Guid.NewGuid()}.mp3");
            //perform the appropriate processing
            switch (urlType) {
                case UrlType.Direct:
                    outputFile = await new DirectDownloader().DownloadFromUrl(url, outputFile, callbackUrl, null);
                    break;
                default:
                    throw new UnknownUrlTypeException($"URL: {url}\n\tcannot be processed");
            }

            //and here is where it gets tricky
            //  upload it to the appropriate target
            return !string.IsNullOrEmpty(outputFile) &&
                   System.IO.File.Exists(outputFile);
        }
    }
}
