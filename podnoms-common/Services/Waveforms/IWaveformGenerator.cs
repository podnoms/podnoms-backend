using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            _logger.LogInformation($"Generating waveform for {remoteUrl}");
            var tempFile = await HttpUtils.DownloadFile(
                remoteUrl,
                $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.mp3"
            );

            _logger.LogInformation($"Downloaded to {tempFile}");
            if (System.IO.File.Exists(tempFile)) {
                _logger.LogInformation($"Context switch to local");
                return await GenerateWaveformLocalFile(tempFile);
            }
            return (string.Empty, string.Empty, string.Empty);
        }

        public async Task<(string, string, string)> GenerateWaveformLocalFile(string localFile) {

            _logger.LogInformation($"Generating waveform for {localFile}");
            var datFile = $"{Path.GetTempPath() + Guid.NewGuid().ToString()}.dat";
            var jsonFile = $"{Path.GetTempPath() + Guid.NewGuid().ToString()}.json";
            var pngFile = $"{Path.GetTempPath() + Guid.NewGuid().ToString()}.png";

            var command = Cli.Wrap(_helpersSettings.WaveformGenerator);

            _logger.LogInformation($"Command is {command.ToString()}");
            var datResult = await command
                .SetArguments($"-i {localFile} -o {datFile} -b 8")
                .ExecuteAsync();

            var jsonArgs = $"-i {localFile} -o {jsonFile} --pixels-per-second 3 -b 8";
            _logger.LogInformation($"JSON args {jsonArgs}");
            var jsonResult = await command
                .SetArguments(jsonArgs)
                .ExecuteAsync();

            try {
                var pngResult = await command
                    .SetArguments($"-i {localFile} -o {pngFile} -b 8 --no-axis-labels --colors audition --waveform-color baacf1FF --background-color 00000000")
                    .ExecuteAsync();
            } catch (Exception e) {
                _logger.LogDebug(e.Message);
            }
            return (
                File.Exists(datFile) ? datFile : string.Empty,
                File.Exists(jsonFile) ? jsonFile : string.Empty,
                File.Exists(pngFile) ? pngFile : string.Empty
            );
        }
    }
}
