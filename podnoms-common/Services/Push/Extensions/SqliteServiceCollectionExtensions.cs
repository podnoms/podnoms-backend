using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push.Extensions {
    public static class SqliteServiceCollectionExtensions {
        private const string SQLITE_CONNECTION_STRING_NAME = "PushSubscriptionSqliteDatabase";

        public static IServiceCollection AddSqlitePushSubscriptionStore(
                    this IServiceCollection services,
                    IConfiguration configuration) {
            services.AddDbContext<PushSubscriptionContext>(options =>
                options.UseSqlite(configuration.GetConnectionString(SQLITE_CONNECTION_STRING_NAME))
            );

            //TODO: This shouldn't be a singleton
            //TODO: See note in NotifyJobCompleteService:54
            services.AddScoped<IPushSubscriptionStore, SqlitePushSubscriptionStore>();

            return services;
        }
    }
}
