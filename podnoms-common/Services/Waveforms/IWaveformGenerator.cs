using System;
using System.Threading.Tasks;
using CliWrap;
using PodNoms.Common.Utils;

namespace PodNoms.Common.Services.Waveforms {
    public interface IWaveformGenerator {
        Task<(string, string, string)> GenerateWaveformLocalFile(string audioFile);
        Task<(string, string, string)> GenerateWaveformRemoteFile(string audioFile);
    }
    public class AWFWaveformGenerator : IWaveformGenerator {
        public Task<(string, string, string)> GenerateWaveformLocalFile(string audioFile) {
            throw new System.NotImplementedException();
        }

        public async Task<(string, string, string)> GenerateWaveformRemoteFile(string audioFile) {

            var tempFile = await HttpUtils.DownloadFile(
                audioFile,
                $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.mp3");
            if (System.IO.File.Exists(tempFile)) {
                var datFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.dat";
                var jsonFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.json";
                var pngFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.png";

                var command = Cli.Wrap("audiowaveform");

                var datResult = await command
                    .SetArguments($"-i {tempFile} -o {datFile} -b 8")
                    .ExecuteAsync();

                var jsonResult = await command
                    .SetArguments($"-i {tempFile} -o {jsonFile} -b 8 --pixels-per-second 20")
                    .ExecuteAsync();

                var pngResult = await command
                    .SetArguments($"-i {tempFile} -o {pngFile} -b 8 --no-axis-labels --colors audition --waveform-color baacf1FF --background-color 00000000")
                    .ExecuteAsync();

                return (
                    datResult.ExitCode == 0 ? datFile : string.Empty,
                    jsonResult.ExitCode == 0 ? jsonFile : string.Empty,
                    pngResult.ExitCode == 0 ? pngFile : string.Empty);
            }
            return (string.Empty, string.Empty, string.Empty);
        }
    }
}
