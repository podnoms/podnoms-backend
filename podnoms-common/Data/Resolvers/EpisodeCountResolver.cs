using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class EpisodeCountResolver : IValueResolver<ApplicationUser, ProfileViewModel, int> {
        private readonly IRepoAccessor _repo;

        public EpisodeCountResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public int Resolve(ApplicationUser source, ProfileViewModel destination, int destMember,
            ResolutionContext context) {
            var results = AsyncHelper.RunSync(() => _repo.Entries.GetAllForUserAsync(source.Id));
            return results.Count();
        }
    }
}
