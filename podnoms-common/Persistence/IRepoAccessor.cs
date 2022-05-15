using System.Threading.Tasks;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence {
    public interface IRepoAccessor {
        PodNomsDbContext Context { get; }

        IPodcastRepository Podcasts { get; }
        IEntryRepository Entries { get; }
        IActivityLogPodcastEntryRepository ActivityLogPodcastEntry { get; }
        ICategoryRepository Categories { get; }
        ITagRepository Tags { get; }
        IPlaylistRepository Playlists { get; }
        IChatRepository Chats { get; }
        INotificationRepository Notifications { get; }
        IApiKeyRepository ApiKey { get; }
        IServiceApiKeyRepository ServiceApiKey { get; }
        IServiceApiKeyLoggerRepository ServiceApiKeyLogger { get; }
        IPaymentRepository Payments { get; }
        IDonationRepository Donations { get; }

        IRepository<T> CreateProxy<T>() where T : BaseEntity;
        Task<bool> CompleteAsync();
    }
}
