using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Extensions;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/radio")]
    [EnableCors("PublicApiPolicy")]
    public class RadioController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly IMapper _mapper;

        public RadioController(IEntryRepository entryRepository,
            IOptions<AppSettings> appSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IMapper mapper) {
            _entryRepository = entryRepository;
            _appSettings = appSettings.Value;
            _storageSettings = storageSettings.Value;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
            _mapper = mapper;
        }

        [HttpGet("playlist")]
        public async Task<string> GetPlaylist() {
            var items = await _entryRepository.GetRandomPlaylistItems();
            var result = string.Join(Environment.NewLine, items.Select(i =>
                i.GetRawAudioUrl(_storageSettings.CdnUrl, _audioFileStorageSettings.ContainerName, "mp3")));
            return result;
        }

        [HttpGet("nowplaying")]
        public async Task<ActionResult<string>> GetNowPlaying([FromQuery]string url) {
            var content = await HttpUtils.DownloadText(url, "application/xml");
            if (!string.IsNullOrEmpty(content)) {
                var result = JsonConvert.DeserializeObject<IcecastResult>(content);
                return Ok(result.icestats.source.title.Truncate(45));
            }
            return NotFound();
        }
    }
}
