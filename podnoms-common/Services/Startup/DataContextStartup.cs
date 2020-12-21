using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Persistence;

namespace PodNoms.Common.Services.Startup {
    public static class DataContextStartup {
        public static IServiceCollection
            AddPodNomsDataContext(this IServiceCollection services, IConfiguration config) {
            services.AddDbContext<PodNomsDbContext>(options => {
                options.UseLazyLoadingProxies();
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("podnoms-common")
                        .EnableRetryOnFailure());
            });
            return services;
        }
    }
}
