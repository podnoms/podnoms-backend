using System;
using System.Threading.Tasks;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;

namespace PodNoms.Tests.Mocks {
    public class TestExternalServiceRequestLogger : IExternalServiceRequestLogger {
        public async Task<ServicesApiKeyLog> LogRequest(ServiceApiKey apiKey, string requesterId, string stackTrace) {
            return await Task.FromResult(new ServicesApiKeyLog {
                Id = Guid.NewGuid(),
                Stack = Guid.NewGuid().ToString(),
                ApiKey = new ServiceApiKey {
                    Id = Guid.NewGuid(),
                    Description = "TestKey",
                    Key = Guid.NewGuid().ToString(),
                    Tainted = false
                },
                RequesterId = Guid.NewGuid().ToString(),
                CreateDate = DateTime.Today,
                UpdateDate = DateTime.Today
            });
        }
    }
}
