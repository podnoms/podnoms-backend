using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IServiceApiKeyRepository : IRepository<ServiceApiKey> {
    }

    internal class ServiceApiKeyRepository : GenericRepository<ServiceApiKey>, IServiceApiKeyRepository {
        public ServiceApiKeyRepository(PodNomsDbContext context, ILogger logger) : base(
            context, logger) {
        }
    }

    public interface IServiceApiKeyLoggerRepository : IRepository<ServicesApiKeyLog> {
    }

    internal class
        ServiceApiKeyLoggerRepository : GenericRepository<ServicesApiKeyLog>, IServiceApiKeyLoggerRepository {
        public ServiceApiKeyLoggerRepository(PodNomsDbContext context, ILogger logger)
            : base(context, logger) {
        }
    }
}
