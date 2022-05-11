using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IDonationRepository : IRepository<Donation> {
    }

    internal class DonationRepository : GenericRepository<Donation>, IDonationRepository {
        public DonationRepository(PodNomsDbContext context, ILogger logger) :
            base(context, logger) {
        }
    }
}
