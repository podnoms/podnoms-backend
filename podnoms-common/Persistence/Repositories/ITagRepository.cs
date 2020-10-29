using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface ITagRepository : IRepository<EntryTag> {
    }

    public class TagRepository : GenericRepository<EntryTag>, ITagRepository {
        public TagRepository(PodNomsDbContext context, ILogger<TagRepository> logger) : base(context, logger) {
        }
    }
}
