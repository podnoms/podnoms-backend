using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Services.Hubs;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence {
    public class UnitOfWork : IUnitOfWork {
        private readonly PodNomsDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly HubLifetimeManager<EntityUpdatesHub> _hub;

        public UnitOfWork(PodNomsDbContext context, ILogger<UnitOfWork> logger,
            HubLifetimeManager<EntityUpdatesHub> hub) {
            _logger = logger;
            _hub = hub;
            _context = context;
        }

        public async Task<bool> CompleteAsync() {
            try {
                await _notifyHubs();
                await _context.SaveChangesAsync();
                return true;
            } catch (DbUpdateException e) {
                _logger.LogError(13756, e, "Error completing unit of work");
                throw;
            }
        }

        private async Task _notifyHubs() {
            var newEntities = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Where(e => e.Entity is IHubNotifyEntity)
                .Select(e => e.Entity as IHubNotifyEntity);

            foreach (var entity in newEntities) {
                var method = entity?.GetHubMethodName();
                if (string.IsNullOrEmpty(method)) {
                    continue;
                }

                var user = entity.UserIdForRealtime(_context);

                if (string.IsNullOrEmpty(user)) {
                    continue;
                }

                var payload = entity?.SerialiseForHub();
                await _hub.SendUserAsync(
                    user,
                    method, new object[] {payload});
            }
        }
    }
}
