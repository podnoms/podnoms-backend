using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Persistence {
    public class UnitOfWork : IUnitOfWork {
        private readonly PodNomsDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        public UnitOfWork(PodNomsDbContext context, ILogger<UnitOfWork> logger) {
            _logger = logger;
            _context = context;
        }
        public async Task<bool> CompleteAsync() {
            try {
                await Task.FromResult<object>(null);
                await _context.SaveChangesAsync();
                return true;
            } catch (DbUpdateException e) {
                _logger.LogError($"Error completing unit of work: {e.Message}\n{e.InnerException.Message}");
                throw e;
            }
        }
    }
}
