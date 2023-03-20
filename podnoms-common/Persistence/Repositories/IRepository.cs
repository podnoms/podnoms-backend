using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IRepository<TEntity> where TEntity : class {
        IQueryable<TEntity> GetAll();
        Task<TEntity> GetAsync(string id);
        Task<TEntity> GetAsync(Guid id);
        Task<TEntity> GetReadOnlyAsync(string id);
        Task<TEntity> GetReadOnlyAsync(Guid id);

        TEntity Create(TEntity entity);
        TEntity Update(TEntity entity);
        Task<TEntity> AddOrUpdate(TEntity entity);
        Task<TEntity> AddOrUpdate(TEntity entity, Expression<Func<TEntity, bool>> predicate);
        Task DeleteAsync(string id);
        Task DeleteAsync(Guid id);
    }

    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity {
        private readonly PodNomsDbContext _context;
        protected readonly ILogger _logger;

        public GenericRepository(PodNomsDbContext context, ILogger logger) {
            _context = context;
            _logger = logger;
        }

        protected PodNomsDbContext GetContext() {
            return _context;
        }

        public virtual IQueryable<TEntity> GetAll() {
            return _context.Set<TEntity>();
        }

        public async Task<TEntity> GetAsync(string id) {
            return await GetAsync(Guid.Parse(id));
        }

        public async Task<TEntity> GetAsync(Guid id) {
            return await _context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<TEntity> GetReadOnlyAsync(string id) {
            return await GetReadOnlyAsync(Guid.Parse(id));
        }

        public async Task<TEntity> GetReadOnlyAsync(Guid id) {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public TEntity Create(TEntity entity) {
            var ret = _context.Set<TEntity>().Add(entity);
            return entity;
        }

        public TEntity Update(TEntity entity) {
            _context.Set<TEntity>().Update(entity);
            return entity;
        }

        public virtual async Task<TEntity> AddOrUpdate(TEntity entity) {
            return await AddOrUpdate(entity, t => t.Id.Equals(entity.Id));
        }

        public virtual async Task<TEntity> AddOrUpdate(TEntity entity, Expression<Func<TEntity, bool>> predicate) {
            try {
                var result = await _context.Set<TEntity>()
                    .AsNoTracking()
                    .Where(predicate)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();
                if (result.Equals(Guid.Empty)) {
                    Create(entity);
                } else {
                    entity.Id = result;
                    Update(entity);
                }

                return entity;
            } catch (Exception e) {
                _logger.LogError("Error adding entity {Message}", e.Message);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id) {
            var entity = await _context.Set<TEntity>().FindAsync(id);
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task DeleteAsync(string id) {
            var entity = await _context.Set<TEntity>().FindAsync(Guid.Parse(id));
            _context.Set<TEntity>().Remove(entity);
        }
    }
}
