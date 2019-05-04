using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
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
