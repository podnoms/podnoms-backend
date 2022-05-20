using System;
using System.Linq;
using System.Threading.Tasks;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Tests.Mocks {
    public class TestApiKeyRepository : IApiKeyRepository {
        public IQueryable<ServiceApiKey> GetAll() {
            throw new NotImplementedException();
        }

        public Task<ServiceApiKey> GetAsync(string id) {
            throw new NotImplementedException();
        }

        public Task<ServiceApiKey> GetAsync(Guid id) {
            throw new NotImplementedException();
        }

        public Task<ServiceApiKey> GetReadOnlyAsync(string id) {
            throw new NotImplementedException();
        }

        public Task<ServiceApiKey> GetReadOnlyAsync(Guid id) {
            throw new NotImplementedException();
        }

        public ServiceApiKey Create(ServiceApiKey entity) {
            throw new NotImplementedException();
        }

        public ServiceApiKey Update(ServiceApiKey entity) {
            throw new NotImplementedException();
        }

        public ServiceApiKey AddOrUpdate(ServiceApiKey entity) {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id) {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id) {
            throw new NotImplementedException();
        }

        public PodNomsDbContext GetContext() {
            throw new NotImplementedException();
        }

        public async Task<ServiceApiKey> GetKey(string type, string userId) {
            var result = new ServiceApiKey {
                Id = System.Guid.NewGuid(),
                Description = "Test API Key",
                Key = Environment.GetEnvironmentVariable("PODNOMS_API_TESTING_YTKEY"),
                Tainted = false
            };
            return await Task.FromResult(result);
        }

        public Task TaintKey(ServiceApiKey key, int taintDays = 7, string reason = "") {
            return Task.CompletedTask;
        }
    }
}
