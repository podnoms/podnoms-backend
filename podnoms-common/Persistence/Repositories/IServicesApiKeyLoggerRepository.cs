using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IServicesApiKeyLoggerRepository : IRepository<ServicesApiKeyLog> {
    }

    public class
        ServicesApiKeyLoggerRepository : GenericRepository<ServicesApiKeyLog>, IServicesApiKeyLoggerRepository {
        public ServicesApiKeyLoggerRepository(PodNomsDbContext context, ILogger<IRepository<ServicesApiKeyLog>> logger)
            : base(context, logger) {
        }
    }
}
