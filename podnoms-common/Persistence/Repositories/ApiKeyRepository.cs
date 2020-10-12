using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IApiKeyRepository : IRepository<ServicesApiKey> {
        Task<string> GetApiKey(string type);
    }

    public class ApiKeyRepository : GenericRepository<ServicesApiKey>, IApiKeyRepository {
        public ApiKeyRepository(PodNomsDbContext context, ILogger<GenericRepository<ServicesApiKey>> logger) : base(
            context, logger) {
        }

        public async Task<string> GetApiKey(string type) {
            var result = await GetAll()
                .Where(k => k.Type.Equals(type))
                .OrderBy(r => Guid.NewGuid())
                .Select(r => r.Key)
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
