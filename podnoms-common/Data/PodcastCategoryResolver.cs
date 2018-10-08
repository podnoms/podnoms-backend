using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class PodcastCategoryResolver : IMemberValueResolver<PodcastViewModel, Podcast, string, Category> {
        private readonly ICategoryRepository _categoryRepository;

        public PodcastCategoryResolver(ICategoryRepository categoryRespository) {
            _categoryRepository = categoryRespository;
        }

        public Category Resolve(PodcastViewModel source, Podcast destination, string sourceMember, Category destMember,
            ResolutionContext context) {
            if (string.IsNullOrEmpty(sourceMember)) return null;

            var category = _categoryRepository
                .GetAll()
                .FirstOrDefault(r => r.Id.ToString() == sourceMember);

            return category;
        }
    }

    internal class PodcastSubcategoryResolver : IMemberValueResolver<PodcastViewModel, Podcast,
        ICollection<SubcategoryViewModel>, ICollection<Subcategory>> {
        private readonly ICategoryRepository _categoryRepository;

        public PodcastSubcategoryResolver(ICategoryRepository categoryRespository) {
            _categoryRepository = categoryRespository;
        }

        public ICollection<Subcategory> Resolve(PodcastViewModel source, Podcast destination,
            ICollection<SubcategoryViewModel> sourceMember, ICollection<Subcategory> destMember,
            ResolutionContext context) {
            var results = _categoryRepository.GetAllSubcategories()
                .Where(r => sourceMember.Select(e => e.Id.ToString()).Contains(r.Id.ToString()))
                .ToList();
            return results;
        }
    }
}