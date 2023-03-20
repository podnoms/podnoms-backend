using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Hubs;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence {
    public class RepoAccessor : IRepoAccessor {
        private readonly PodNomsDbContext _context;

        #region Repositories

        public IPodcastRepository Podcasts { get; }
        public IEntryRepository Entries { get; }
        public IActivityLogPodcastEntryRepository ActivityLogPodcastEntry { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public ITagRepository Tags { get; private set; }
        public IPlaylistRepository Playlists { get; private set; }
        public IChatRepository Chats { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public IApiKeyRepository ApiKey { get; private set; }
        public IServiceApiKeyRepository ServiceApiKey { get; private set; }
        public IServiceApiKeyLoggerRepository ServiceApiKeyLogger { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public IDonationRepository Donations { get; private set; }

        #endregion


        private readonly ILogger<RepoAccessor> _logger;
        private readonly HubLifetimeManager<EntityUpdatesHub> _hub;


        public RepoAccessor(PodNomsDbContext context, ILogger<RepoAccessor> logger,
            IPodcastRepository podcastRepository,
            IEntryRepository entryRepository,
            HubLifetimeManager<EntityUpdatesHub> hub) {
            _logger = logger;
            _hub = hub;
            _context = context;


            Podcasts = podcastRepository;
            Entries = entryRepository;

            Categories = new CategoryRepository(_context, _logger);
            Tags = new TagRepository(_context, _logger);
            Playlists = new PlaylistRepository(_context, _logger);
            Chats = new ChatRepository(_context, _logger);
            Notifications = new NotificationRepository(_context, _logger);

            ApiKey = new ApiKeyRepository(_context, _logger);
            ServiceApiKey = new ServiceApiKeyRepository(_context, _logger);
            ServiceApiKeyLogger = new ServiceApiKeyLoggerRepository(_context, _logger);

            ActivityLogPodcastEntry = new ActivityLogPodcastEntryRepository(_context, _logger);
            Payments = new PaymentRepository(_context, _logger);
            Donations = new DonationRepository(_context, _logger);
        }

        public PodNomsDbContext Context => _context;

        public IRepository<T> CreateProxy<T>() where T : BaseEntity {
            return new GenericRepository<T>(_context, _logger);
        }

        public async Task<bool> CompleteAsync() {
            try {
                await _notifyHubs();
                await _context.SaveChangesAsync();
                return true;
            } catch (DbUpdateException e) {
                _logger.LogError(13756, e, "Error completing unit of work");
                throw;
            }
        }

        private async Task _notifyHubs() {
            var newEntities = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Where(e => e.Entity is IHubNotifyEntity)
                .Select(e => e.Entity as IHubNotifyEntity);

            foreach (var entity in newEntities) {
                var method = entity?.GetHubMethodName();
                if (string.IsNullOrEmpty(method)) {
                    continue;
                }

                var user = entity.UserIdForRealtime(_context);

                if (string.IsNullOrEmpty(user)) {
                    continue;
                }

                var payload = entity?.SerialiseForHub();
                await _hub.SendUserAsync(
                    user,
                    method, new object[] {payload});
            }
        }
    }
}
