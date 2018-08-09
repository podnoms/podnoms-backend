using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Models;

namespace PodNoms.Api.Persistence {
    public interface ICategoryRepository : IRepository<Category> {
        List<Subcategory> GetAllSubcategories();
    }

    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository {
        public CategoryRepository(PodNomsDbContext context, ILogger<GenericRepository<Category>> logger) : base(context, logger) {
        }

        public List<Subcategory> GetAllSubcategories() {
            return GetContext().Subcategories.ToList();
        }
    }
}
