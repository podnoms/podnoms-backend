using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Crypt;
using PodNoms.Data.Models;
using PodNoms.Data.Utils;

namespace PodNoms.Common.Auth.ApiKeys {
    public class IssuedKeysGetApiKeyQuery : IGetApiKeyQuery {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _config;

        public IssuedKeysGetApiKeyQuery(IServiceScopeFactory serviceScopeFactory,
                                        IConfiguration config) {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
        }
        public async Task<ApplicationUser> Execute(string providedApiKey) {
            // so we need to salt the provided key with the
            // single salt we have for all API Keys
            var salt = _config.GetSection("ApiKeyAuthSettings")["ApiKeySalt"];
            var storedPassword = ApiKeyGenerator.GeneratePasswordHash(providedApiKey, salt);
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var issuedApiKeyRepository = serviceScope
                .ServiceProvider
                .GetRequiredService<IRepository<IssuedApiKey>>();

            var record = await issuedApiKeyRepository
                .GetAll()
                .Include(r => r.IssuedTo)
                .Where(r => r.Key.Equals(storedPassword))
                .SingleOrDefaultAsync();
                
            return record.IssuedTo;
        }
    }
}
