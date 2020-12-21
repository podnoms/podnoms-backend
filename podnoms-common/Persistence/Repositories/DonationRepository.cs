using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IDonationRepository : IRepository<Donation> {

    }
    public class DonationRepository : GenericRepository<Donation>, IDonationRepository {
        public DonationRepository(PodNomsDbContext context, ILogger<GenericRepository<Donation>> logger) :
                base(context, logger) {
        }
    }
}
