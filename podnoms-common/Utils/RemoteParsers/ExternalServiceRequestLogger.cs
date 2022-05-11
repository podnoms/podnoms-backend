using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Utils.RemoteParsers {
    public interface IExternalServiceRequestLogger {
        Task<ServicesApiKeyLog> LogRequest(ServiceApiKey apiKey, string requesterId, string stackTrace);
    }

    public class ExternalServiceRequestLogger : IExternalServiceRequestLogger {
        private readonly IServiceApiKeyLoggerRepository _repository;
        private readonly IRepoAccessor _repoAccessor;
        private readonly ILogger<ExternalServiceRequestLogger> _logger;

        public ExternalServiceRequestLogger(IServiceApiKeyLoggerRepository repository, IRepoAccessor repoAccessor,
            ILogger<ExternalServiceRequestLogger> logger) {
            _repository = repository;
            _repoAccessor = repoAccessor;
            _logger = logger;
        }

        public async Task<ServicesApiKeyLog> LogRequest(ServiceApiKey apiKey, string requesterId, string stackTrace) {
            try {
                var log = apiKey.LogRequest(requesterId, stackTrace);
                await _repoAccessor.CompleteAsync();
                return log;
            } catch (Exception e) when (e is NullReferenceException or DbUpdateException) {
                _logger.LogError("Error saving external service request");
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}
