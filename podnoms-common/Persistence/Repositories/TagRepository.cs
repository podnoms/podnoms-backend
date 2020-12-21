using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PodNoms.Common.Persistence.Repositories {
    public interface ITagRepository : IRepository<EntryTag> {
        Task<ICollection<EntryTag>> UpdateAndMerge(ICollection<EntryTag> entityTags);
    }

    public class TagRepository : GenericRepository<EntryTag>, ITagRepository {
        public TagRepository(PodNomsDbContext context, ILogger<TagRepository> logger) : base(context, logger) {
        }

        public async Task<ICollection<EntryTag>> UpdateAndMerge(ICollection<EntryTag> entityTags) {
            foreach (var tag in entityTags) {
                if (!tag.Id.Equals(Guid.Empty)) {
                    continue;
                }

                //format and find tag
                var formattedTag = tag.TagName.ToTitleCase();
                var existing = await GetAll()
                    .Where(r => r.TagName.Equals(formattedTag))
                    .FirstOrDefaultAsync();
                if (existing != null) {
                    tag.Id = existing.Id;
                    tag.TagName = existing.TagName;
                }

                AddOrUpdate(tag);
            }

            //gotta do this to avoid the classic
            //The instance of entity type 'EntryTag' cannot be tracked because another instance
            //with the same key value for {'Id'} is already being tracked
            await GetContext().SaveChangesAsync();
            return entityTags;
        }
    }
}
