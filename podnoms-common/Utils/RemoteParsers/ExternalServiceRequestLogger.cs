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
        private readonly IServicesApiKeyLoggerRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExternalServiceRequestLogger> _logger;

        public ExternalServiceRequestLogger(IServicesApiKeyLoggerRepository repository, IUnitOfWork unitOfWork,
            ILogger<ExternalServiceRequestLogger> logger) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServicesApiKeyLog> LogRequest(ServiceApiKey apiKey, string requesterId, string stackTrace) {
            try {
                var log = apiKey.LogRequest(requesterId, stackTrace);
                await _unitOfWork.CompleteAsync();
                return log;
            } catch (Exception e) when (e is NullReferenceException or DbUpdateException) {
                _logger.LogError("Error saving external service request");
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}
