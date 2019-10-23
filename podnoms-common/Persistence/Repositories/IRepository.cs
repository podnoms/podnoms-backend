using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Interfaces;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IRepository<TEntity> where TEntity : class, IEntity {
        IQueryable<TEntity> GetAll();
        Task<TEntity> GetAsync(string id);
        Task<TEntity> GetAsync(Guid id);
        TEntity Create(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity AddOrUpdate(TEntity entity);
        Task DeleteAsync(string id);
        Task DeleteAsync(Guid id);
    }

    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity {
        private PodNomsDbContext _context;
        private readonly ILogger<GenericRepository<TEntity>> _logger;

        public GenericRepository(PodNomsDbContext context, ILogger<GenericRepository<TEntity>> logger) {
            _context = context;
            _logger = logger;
        }

        public PodNomsDbContext GetContext() {
            return _context;
        }

        public IQueryable<TEntity> GetAll() {
            return _context.Set<TEntity>();
        }
        public async Task<TEntity> GetAsync(string id) {
            return await _context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id.ToString() == id);
        }
        public async Task<TEntity> GetAsync(Guid id) {
            return await _context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public TEntity Create(TEntity entity) {
            var ret = _context.Set<TEntity>().Add(entity);
            return entity;
        }

        public TEntity Update(TEntity entity) {
            var ret = _context.Set<TEntity>().Update(entity);
            return entity;
        }

        public virtual TEntity AddOrUpdate(TEntity entity) {
            var ret = entity;
            // TODO: Fix this logic, we can no longer guarantee blanks IDs for new records
            ret = entity.Id != Guid.Empty ? Update(entity) : Create(entity);
            return ret;
        }

        public async Task DeleteAsync(Guid id) {
            var entity = await _context.Set<TEntity>().FindAsync(id);
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task DeleteAsync(string id) {
            if (Guid.TryParse(id, out Guid guid)) {
                var entity = await _context.Set<TEntity>().FindAsync(id);
                _context.Set<TEntity>().Remove(entity);
            } else {
                _logger.LogError($"Error updating entity with guid: {id}");
            }
        }
    }
}
