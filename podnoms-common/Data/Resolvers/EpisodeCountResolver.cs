using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers
{
    internal class EpisodeCountResolver : IValueResolver<ApplicationUser, ProfileViewModel, int> {
        private readonly IEntryRepository _entryRepository;

        public EpisodeCountResolver(IEntryRepository entryRepository) {
            _entryRepository = entryRepository;
        }

        public int Resolve(ApplicationUser source, ProfileViewModel destination, int destMember,
            ResolutionContext context) {
            var results = AsyncHelper.RunSync(() => _entryRepository.GetAllForUserAsync(source.Id));
            return results.Count();
        }
    }
}
