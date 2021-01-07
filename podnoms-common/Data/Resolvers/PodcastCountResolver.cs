using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class PodcastCountResolver : IValueResolver<ApplicationUser, ProfileViewModel, int> {
        private readonly IPodcastRepository _podcastRepository;

        public PodcastCountResolver(IPodcastRepository podcastRepository) {
            _podcastRepository = podcastRepository;
        }

        public int Resolve(ApplicationUser source, ProfileViewModel destination, int destMember,
            ResolutionContext context) {
            var results = AsyncHelper.RunSync(() => _podcastRepository.GetAllForUserAsync(source.Id));
            return results.Count();
        }
    }
}
