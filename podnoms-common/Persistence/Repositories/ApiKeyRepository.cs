using System;
using System.Linq;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IApiKeyRepository : IRepository<ServiceApiKey> {
        Task<ServiceApiKey> GetKey(string type, string userId);
        Task TaintKey(ServiceApiKey key, int taintDays = 7, string reason = "");
    }

    internal class ApiKeyRepository : GenericRepository<ServiceApiKey>, IApiKeyRepository {
        public ApiKeyRepository(PodNomsDbContext context, ILogger logger) : base(
            context, logger) {
        }

        public async Task<ServiceApiKey> GetKey(string type, string userId) {
            var result = await GetAll()
                .Where(u => u.ApplicationUserId.ToString().Equals(userId) ||
                            string.IsNullOrEmpty(u.ApplicationUserId.ToString()))
                .Where(k => k.Type.Equals(type))
                .Where(k => k.Enabled)
                .Where(k => k.TaintedDate == null ||
                            k.TaintedDate <= System.DateTime.Now.AddDays(ServiceApiKey.TAINT_LENGTH * -1))
                .OrderByDescending(r => r.ApplicationUserId)
                .ThenBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();
            return result;
        }

        public async Task TaintKey(ServiceApiKey key, int taintDays = 7, string reason = "") {
            var record = await GetAll()
                .Where(k => k.Key.Equals(key.Key))
                .FirstOrDefaultAsync();
            if (record is null)
                return;

            record.Tainted = true;
            record.TaintedDate = DateTime.Now;
            record.TaintedReason = reason;
            await GetContext().SaveChangesAsync();
        }
    }
}
