using Autofac.Core;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IServiceApiKeyRepository : IRepository<ServiceApiKey> {
    }

    public class ServiceApiKeyRepository : GenericRepository<ServiceApiKey>, IServiceApiKeyRepository {
        public ServiceApiKeyRepository(PodNomsDbContext context, ILogger<IRepository<ServiceApiKey>> logger) : base(
            context, logger) {
        }
    }

    public interface IServicesApiKeyLoggerRepository : IRepository<ServicesApiKeyLog> {
    }

    public class
        ServicesApiKeyLoggerRepository : GenericRepository<ServicesApiKeyLog>, IServicesApiKeyLoggerRepository {
        public ServicesApiKeyLoggerRepository(PodNomsDbContext context, ILogger<IRepository<ServicesApiKeyLog>> logger)
            : base(context, logger) {
        }
    }
}

