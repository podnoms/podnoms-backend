using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Jobs {
    public class CheckItemImagesJob : IHostedJob {
        public readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RemoteImageCacher _imageCacher;
        private readonly IYouTubeParser _youTubeParser;
        public readonly StorageSettings _storageSettings;
        public readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly ILogger<CheckAudioExistsJob> _logger;
        private readonly IMailSender _mailSender;

        public CheckItemImagesJob(IEntryRepository entryRepository, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioStorageSettings, IUnitOfWork unitOfWork,
            RemoteImageCacher imageCacher, IYouTubeParser youTubeParser, ILoggerFactory logger, IMailSender mailSender) {
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
            _imageCacher = imageCacher;
            _youTubeParser = youTubeParser;
            _logger = logger.CreateLogger<CheckAudioExistsJob>();
        }

        public async Task<bool> Execute() {
            return await Execute(null);
        }

        public async Task<bool> Execute(PerformContext context) {
            //get all the unprocessed images
            var entries = await _entryRepository.GetAll()
                .Where(x => x.ImageUrl.StartsWith("http"))
                .Where(x => x.ImageUrl.Equals("https://img.youtube.com/vi/-Zr_aNXS2RE/sddefault.jpg"))
                .ToListAsync();
            foreach (var entry in entries) {
                var file = await _imageCacher.CacheImage(entry.ImageUrl, entry.Id.ToString());
                if (string.IsNullOrEmpty(file)) {
                    if (_youTubeParser.ValidateUrl(entry.SourceUrl)) {
                        var info = await _youTubeParser.GetInformation(entry.SourceUrl);
                        file = await _imageCacher.CacheImage(info.Thumbnail, entry.Id.ToString());
                    }
                }
                if (!string.IsNullOrEmpty(file)) {
                    entry.ImageUrl = file;
                }
            }
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
