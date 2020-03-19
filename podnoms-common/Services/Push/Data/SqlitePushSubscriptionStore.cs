using System;
using System.Linq;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;

namespace PodNoms.Common.Services.Push.Data {
    internal class SqlitePushSubscriptionStore : IPushSubscriptionStore {
        private readonly PushSubscriptionContext _context;

        public SqlitePushSubscriptionStore(PushSubscriptionContext context) {
            _context = context;
        }

        public PushSubscriptionContext GetContext() => _context;

        public async Task<string> StoreSubscriptionAsync(string uid, PushSubscription subscription) {
            var entity = new PushSubscriptionContext.PushSubscription(uid, subscription);
            if (_context.Subscriptions.Where(s => s.Endpoint == subscription.Endpoint).Count() > 0) {
                // _context.Entry(entry).State = EntityState.Modified
                _context.Subscriptions.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            } else {
                _context.Subscriptions.Add(entity);
            }
            await _context.SaveChangesAsync();

            return uid;
        }

        public async Task DiscardSubscriptionAsync(string endpoint) {
            var subscription = await _context.Subscriptions.FindAsync(endpoint);

            _context.Subscriptions.Remove(subscription);

            await _context.SaveChangesAsync();
        }
        public Task ForEachSubscriptionAsync(string uid, Action<PushSubscription> action) {
            return _context.Subscriptions.Where(e => e.Id == uid).AsNoTracking().ForEachAsync(action);
        }
        public Task ForEachSubscriptionAsync(Action<PushSubscription> action) {
            return _context.Subscriptions.AsNoTracking().ForEachAsync(action);
        }
    }
}
