using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Extensions;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/radio")]
    public class RadioController : Controller {
        private readonly IRepoAccessor _repo;
        private readonly ILogger<RadioController> _logger;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;

        public RadioController(
            IRepoAccessor repo,
            IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            ILogger<RadioController> logger) {
            _repo = repo;
            _logger = logger;
            _storageSettings = storageSettings.Value;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
        }

        [HttpGet("playlist")]
        public async Task<string> GetPlaylist() {
            var items = await _repo.Entries.GetRandomPlaylistItems();
            var result = string.Join(Environment.NewLine, items.Select(i =>
                i.GetRawAudioUrl(_storageSettings.CdnUrl, _audioFileStorageSettings.ContainerName, "mp3")));
            return result;
        }

        [HttpGet("nowplaying")]
        public async Task<ActionResult<string>> GetNowPlaying([FromQuery] string url) {
            try {
                var content = await HttpUtils.DownloadText(url, "application/xml");
                if (!string.IsNullOrEmpty(content)) {
                    var result = JsonSerializer.Deserialize<IcecastResult>(content);
                    return Ok(result.icestats.source.title.Truncate(45, true));
                }
            } catch (Exception) {
                _logger.LogWarning("Unable to get now playing url");
            }

            return NoContent();
        }
    }
}
