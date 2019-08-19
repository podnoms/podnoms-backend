using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using PodNoms.Common.Utils;

namespace PodNoms.Common.Services.Waveforms {
    public interface IWaveformGenerator {
        Task<(string, string, string)> GenerateWaveformLocalFile(string audioFile);
        Task<(string, string, string)> GenerateWaveformRemoteFile(string audioFile);
    }
    public class AWFWaveformGenerator : IWaveformGenerator {

        public async Task<(string, string, string)> GenerateWaveformRemoteFile(string remoteUrl) {
            var tempFile = await HttpUtils.DownloadFile(
                remoteUrl,
                $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.mp3"
            );

            if (System.IO.File.Exists(tempFile)) {
                return await GenerateWaveformLocalFile(tempFile);
            }
            return (string.Empty, string.Empty, string.Empty);
        }

        public async Task<(string, string, string)> GenerateWaveformLocalFile(string localFile) {

            var datFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.dat";
            var jsonFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.json";
            var pngFile = $"{System.IO.Path.GetTempPath() + Guid.NewGuid().ToString()}.png";

            var command = Cli.Wrap("audiowaveform");

            var datResult = await command
                .SetArguments($"-i {localFile} -o {datFile} -b 8")
                .ExecuteAsync();

            var jsonResult = await command
                .SetArguments($"-i {localFile} -o {jsonFile} -b 8 --pixels-per-second 20")
                .ExecuteAsync();

            try {
                var pngResult = await command
                    .SetArguments($"-i {localFile} -o {pngFile} -b 8 --no-axis-labels --colors audition --waveform-color baacf1FF --background-color 00000000")
                    .ExecuteAsync();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return (
                File.Exists(datFile) ? datFile : string.Empty,
                File.Exists(jsonFile) ? jsonFile : string.Empty,
                File.Exists(pngFile) ? pngFile : string.Empty
            );
        }
    }
}
