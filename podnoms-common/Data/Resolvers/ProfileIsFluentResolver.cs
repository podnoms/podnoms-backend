using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Nito.AsyncEx;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileIsFluentResolver : IValueResolver<ApplicationUser, ProfileViewModel, bool> {
        private readonly IRepoAccessor _repo;

        public ProfileIsFluentResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        private async Task<bool> _runQuery(string slug) {
            var entries = await _repo.Entries.GetAllForUserAsync(slug);
            return entries.Count() > 5;
        }

        public bool Resolve(ApplicationUser source, ProfileViewModel destination, bool destMember,
            ResolutionContext context) {
            return AsyncContext.Run(async () => await _runQuery(source.Id));
        }
    }
}
