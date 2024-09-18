using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Hubs;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence;

public class RepoAccessor : IRepoAccessor {
  private readonly HubLifetimeManager<EntityUpdatesHub> _hub;


  private readonly ILogger<RepoAccessor> _logger;


  public RepoAccessor(PodNomsDbContext context, ILogger<RepoAccessor> logger,
    IPodcastRepository podcastRepository,
    IEntryRepository entryRepository,
    HubLifetimeManager<EntityUpdatesHub> hub) {
    _logger = logger;
    _hub = hub;
    Context = context;


    Podcasts = podcastRepository;
    Entries = entryRepository;

    Categories = new CategoryRepository(Context, _logger);
    Tags = new TagRepository(Context, _logger);
    Playlists = new PlaylistRepository(Context, _logger);
    Chats = new ChatRepository(Context, _logger);
    Notifications = new NotificationRepository(Context, _logger);

    ApiKey = new ApiKeyRepository(Context, _logger);
    ServiceApiKey = new ServiceApiKeyRepository(Context, _logger);
    ServiceApiKeyLogger = new ServiceApiKeyLoggerRepository(Context, _logger);

    ActivityLogPodcastEntry = new ActivityLogPodcastEntryRepository(Context, _logger);
    Payments = new PaymentRepository(Context, _logger);
    Donations = new DonationRepository(Context, _logger);
  }

  public PodNomsDbContext Context { get; }

  public IRepository<T> CreateProxy<T>() where T : BaseEntity {
    return new GenericRepository<T>(Context, _logger);
  }

  public async Task<bool> CompleteAsync() {
    try {
      await _notifyHubs();
      await Context.SaveChangesAsync();
      return true;
    } catch (DbUpdateException e) {
      _logger.LogError(13756, e, "Error completing unit of work");
      throw;
    }
  }

  private async Task _notifyHubs() {
    var newEntities = Context.ChangeTracker.Entries()
      .Where(e => e.State == EntityState.Added)
      .Where(e => e.Entity is IHubNotifyEntity)
      .Select(e => e.Entity as IHubNotifyEntity);

    foreach (var entity in newEntities) {
      var method = entity?.GetHubMethodName();
      if (string.IsNullOrEmpty(method)) {
        continue;
      }

      var user = entity.UserIdForRealtime(Context);

      if (string.IsNullOrEmpty(user)) {
        continue;
      }

      var payload = entity?.SerialiseForHub();
      await _hub.SendUserAsync(
        user,
        method, new object[] { payload });
    }
  }

  #region Repositories

  public IPodcastRepository Podcasts { get; }
  public IEntryRepository Entries { get; }
  public IActivityLogPodcastEntryRepository ActivityLogPodcastEntry { get; }
  public ICategoryRepository Categories { get; }
  public ITagRepository Tags { get; }
  public IPlaylistRepository Playlists { get; }
  public IChatRepository Chats { get; }
  public INotificationRepository Notifications { get; }
  public IApiKeyRepository ApiKey { get; }
  public IServiceApiKeyRepository ServiceApiKey { get; }
  public IServiceApiKeyLoggerRepository ServiceApiKeyLogger { get; }
  public IPaymentRepository Payments { get; }
  public IDonationRepository Donations { get; }

  #endregion
}
