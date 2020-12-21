using System.Threading.Tasks;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Utils.RemoteParsers
{
    public class ExternalServiceRequestLogger {
        private readonly IServicesApiKeyLoggerRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ExternalServiceRequestLogger(IServicesApiKeyLoggerRepository repository, IUnitOfWork unitOfWork) {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServicesApiKeyLog> LogRequest(ServiceApiKey apiKey, string requesterId, string stackTrace) {
            var log = apiKey.LogRequest(requesterId, stackTrace);
            await _unitOfWork.CompleteAsync();
            return log;
        }
    }
}
