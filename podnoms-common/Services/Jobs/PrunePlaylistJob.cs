using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Processor;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public class PrunePlaylistJob : IHostedJob {

        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<ProcessPlaylistItemJob> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public PrunePlaylistJob(
                IPlaylistRepository playlistRepository,
                IAudioUploadProcessService uploadService,
                IOptions<AppSettings> appSettings,
                IPodcastRepository podcastRepository,
                IPaymentRepository paymentRepository,
                IOptions<ImageFileStorageSettings> imageStorageSettings,
                IOptions<StorageSettings> storageSettings,
                IOptions<HelpersSettings> helpersSettings,
                IUnitOfWork unitOfWork,
                ILogger<ProcessPlaylistItemJob> logger) {

            _unitOfWork = unitOfWork;
            _playlistRepository = playlistRepository;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }
        [Mutex("PrunePlaylistJob")]
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [Mutex("PrunePlaylistJob")]
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            var playlists = _playlistRepository.GetOversubscribedPlaylists();

            if (playlists.Count() != 0) {
                foreach (var playlist in playlists) {
                    //check if user has a subscription
                    var subs = _paymentRepository.GetAllValidSubscriptions(playlist.Podcast.AppUser.Id);
                    if (!subs.Any()) {
                        //prune playlists to 10
                        await ExecuteForPlaylist(playlist.Id.ToString());
                    }
                }
                BackgroundJob.Enqueue<DeleteOrphanAudioJob>(s => s.Execute());
            }
            return true;
        }
        public async Task<bool> ExecuteForPlaylist(string playlistId) {
            var entriesToRemove = _playlistRepository
                .GetExpiredItems(playlistId);


            var response = await _playlistRepository.DeleteExpired(playlistId);
            if (response != 0) {
                _logger.LogDebug($"Deleted {response} playlist items from {playlistId}");
                await _unitOfWork.CompleteAsync();
            }
            return true;
        }
    }
}
