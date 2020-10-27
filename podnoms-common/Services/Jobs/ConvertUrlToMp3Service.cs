using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Services.Audio;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.Hubs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Enums;

namespace PodNoms.Common.Services.Jobs {
    public class ConvertUrlToMp3Service : AbstractHostedJob {
        //TODO: This should form a template for replacing the horrid auth token stuff
        //TODO: I'm doing in the existing URL processing stuff

        private readonly HubLifetimeManager<PublicUpdatesHub> _hub;
        private readonly AudioDownloader _downloader;
        private readonly IUrlProcessService _urlProcessService;
        private readonly IFileUploader _fileUploader;
        private readonly IMP3Tagger _tagger;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageSettings;

        public ConvertUrlToMp3Service(
            ILogger<ConvertUrlToMp3Service> logger,
            HubLifetimeManager<PublicUpdatesHub> hub,
            AudioDownloader downloader,
            IOptions<AppSettings> appSettings,
            IOptions<StorageSettings> storageSettings,
            IUrlProcessService urlProcessService,
            IFileUploader fileUploader,
            IMP3Tagger tagger) : base(logger) {
            _hub = hub;
            _downloader = downloader;
            _urlProcessService = urlProcessService;
            _fileUploader = fileUploader;
            _tagger = tagger;
            _appSettings = appSettings.Value;
            _storageSettings = storageSettings.Value;
        }

        public override Task<bool> Execute(PerformContext context) {
            throw new System.NotImplementedException();
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task<bool> ProcessEntry(string url, string processId, PerformContext context) {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{_appSettings.RealtimeUrl}/publicupdates")
                .Build();
            await connection.StartAsync();

            var fileName = $"{processId}.mp3";
            var outputFile = Path.Combine(Path.GetTempPath(), fileName);

            var processResult = await _urlProcessService.DownloadAudioV2(processId, url, outputFile, (e) => {
                connection.InvokeAsync("SendMessage", processId, "processing", e);
                return true;
            });
            if (!processResult || !File.Exists(outputFile)) {
                return false;
            }

            var info = await _downloader.GetInfo(url, string.Empty);

            var localImageFile = await HttpUtils.DownloadFile("https://cdn.podnoms.com/static/images/pn-back.jpg");
            _tagger.CreateTags(
                outputFile,
                localImageFile,
                _downloader.RawProperties?.Title ?? "Downloaded by PodNoms",
                "Downloaded by PodNoms",
                "Downloaded By PodNoms",
                "Downloaded By PodNoms",
                "Downloaded By PodNoms");


            var cdnFilename = $"processed/{fileName}";
            var uploadResult = await _fileUploader.UploadFile(
                outputFile, "public", cdnFilename, "audio/mpeg",
                async (p, t) => {
                    var progress = new ProcessingProgress(new TransferProgress {
                        Percentage = p,
                        TotalSize = t.ToString()
                    }) {
                        Progress = "Caching",
                        ProcessingStatus = ProcessingStatus.Uploading
                    };
                    await connection.InvokeAsync("SendMessage", processId, "processing", progress);
                }
            );
            var message = new ProcessingProgress(null) {
                Payload = Flurl.Url.Combine(_storageSettings.CdnUrl, "public", cdnFilename),
                Progress = "Processed",
                ProcessingStatus = ProcessingStatus.Processed
            };
            await connection.InvokeAsync("SendMessage", processId, "processing", message);
            return true;
        }
    }
}
