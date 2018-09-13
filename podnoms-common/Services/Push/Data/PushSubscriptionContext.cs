using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WP = Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Push.Data {
    internal class PushSubscriptionContext : DbContext {
        public class PushSubscription : WP.PushSubscription {
            public PushSubscription() { }
            public PushSubscription(string id, WP.PushSubscription subscription) {
                Id = id;
                Endpoint = subscription.Endpoint;
                Keys = subscription.Keys;
            }

            public string Id { get; set; }
            public string P256DH {
                get { return GetKey(WP.PushEncryptionKeyName.P256DH); }
                set { SetKey(WP.PushEncryptionKeyName.P256DH, value); }
            }
            public string Auth {
                get { return GetKey(WP.PushEncryptionKeyName.Auth); }
                set { SetKey(WP.PushEncryptionKeyName.Auth, value); }
            }
        }

        public DbSet<PushSubscription> Subscriptions { get; set; }

        public PushSubscriptionContext(DbContextOptions<PushSubscriptionContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var pushSubscriptionEntityTypeBuilder = modelBuilder.Entity<PushSubscription>();
            pushSubscriptionEntityTypeBuilder.HasKey(e => e.Endpoint);
            pushSubscriptionEntityTypeBuilder.Ignore(p => p.Keys);
        }
    }
}
