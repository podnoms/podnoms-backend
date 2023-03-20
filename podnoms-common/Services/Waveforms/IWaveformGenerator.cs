using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.AudioParsing.Helpers;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Utils;

namespace PodNoms.Common.Services.Waveforms {
    public interface IWaveformGenerator {
        Task<(string, string, string)> GenerateWaveformLocalFile(string audioFile);
        Task<(string, string, string)> GenerateWaveformRemoteFile(string audioFile);
    }

    public class AWFWaveformGenerator : IWaveformGenerator {
        private readonly ILogger<AWFWaveformGenerator> _logger;
        private readonly HelpersSettings _helpersSettings;

        public AWFWaveformGenerator(ILogger<AWFWaveformGenerator> logger, IOptions<HelpersSettings> helpersSettings) {
            _logger = logger;
            _helpersSettings = helpersSettings.Value;
        }

        public async Task<(string, string, string)> GenerateWaveformRemoteFile(string remoteUrl) {
            _logger.LogInformation("Generating waveform for {RemoteUrl}", remoteUrl);
            var tempFile = await HttpUtils.DownloadFile(
                remoteUrl,
                $"{Path.Combine(PathUtils.GetScopedTempPath(), Guid.NewGuid().ToString(), ".mp3")}"
            );

            _logger.LogInformation("Downloaded to {TempFile}", tempFile);
            if (System.IO.File.Exists(tempFile)) {
                _logger.LogInformation($"Context switch to local");
                return await GenerateWaveformLocalFile(tempFile);
            }

            return (string.Empty, string.Empty, string.Empty);
        }

        public async Task<(string, string, string)> GenerateWaveformLocalFile(string localFile) {
            _logger.LogInformation("Generating waveform for {LocalFile}", localFile);
            var datFile = PathUtils.GetScopedTempFile("dat");
            var jsonFile = PathUtils.GetScopedTempFile("json");
            var pngFile = PathUtils.GetScopedTempFile("png");

            var command = Cli.Wrap(_helpersSettings.WaveformGenerator);
            _logger.LogInformation("Command is {Command}", command.ToString());
            var datResult = await command
                .WithArguments($"-i {localFile} -o {datFile} -b 8")
                .ExecuteBufferedAsync();
            _logger.LogInformation("DAT result is {DatResultStandardOutput}", datResult.StandardOutput);

            var jsonArgs = $"-i {localFile} -o {jsonFile} --pixels-per-second 3 -b 8";
            _logger.LogInformation("JSON args {JsonArgs}", jsonArgs);
            var jsonResult = await command
                .WithArguments(jsonArgs)
                .ExecuteBufferedAsync();
            _logger.LogInformation("JSON result is {JsonResultStandardOutput}", jsonResult.StandardOutput);

            try {
                var pngResult = await command
                    .WithArguments(
                        $"-i {localFile} -o {pngFile} -b 8 --no-axis-labels --colors audition --waveform-color baacf1FF --background-color 00000000")
                    .ExecuteBufferedAsync();
                _logger.LogInformation("PNG result is {JsonResultStandardOutput}", jsonResult.StandardOutput);
                _logger.LogInformation("PNG error is {JsonResultStandardError}", jsonResult.StandardError);
            } catch (Exception e) {
                _logger.LogDebug("{Message}", e.Message);
            }

            return (
                File.Exists(datFile) ? datFile : string.Empty,
                File.Exists(jsonFile) ? jsonFile : string.Empty,
                File.Exists(pngFile) ? pngFile : string.Empty
            );
        }
    }
}
