using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class PodcastCategoryResolver : IValueResolver<PodcastViewModel, Podcast, Category> {
        private readonly IRepoAccessor _repo;

        public PodcastCategoryResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public Category Resolve(PodcastViewModel source, Podcast destination, Category destMember,
            ResolutionContext context) {
            var category = _repo.Categories
                .GetAll()
                .FirstOrDefault(r => r.Id.Equals(source.Category));

            return category;
        }
    }

    internal class PodcastSubcategoryResolver : IMemberValueResolver<PodcastViewModel, Podcast,
        ICollection<SubcategoryViewModel>, ICollection<Subcategory>> {
        private readonly IRepoAccessor _repo;

        public PodcastSubcategoryResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public ICollection<Subcategory> Resolve(PodcastViewModel source, Podcast destination,
            ICollection<SubcategoryViewModel> sourceMember, ICollection<Subcategory> destMember,
            ResolutionContext context) {
            var results = _repo.Categories.GetAllSubcategories()
                .Where(r => sourceMember.Select(e => e.Id.ToString()).Contains(r.Id.ToString()))
                .ToList();
            return results;
        }
    }
}
