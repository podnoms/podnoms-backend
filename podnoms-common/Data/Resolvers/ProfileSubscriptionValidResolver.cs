using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using System.Linq;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileSubscriptionValidResolver : IValueResolver<ApplicationUser, SubscriptionViewModel, bool> {
        private readonly IRepoAccessor _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileSubscriptionValidResolver(IRepoAccessor repo, UserManager<ApplicationUser> userManager) {
            _repo = repo;
            _userManager = userManager;
        }

        public bool Resolve(ApplicationUser source, SubscriptionViewModel destination, bool destMember,
            ResolutionContext context) {
            var isAdmin = AsyncHelper.RunSync(() => _userManager.IsInRoleAsync(source, "god-mode"));
            if (isAdmin) {
                return true;
            }
            var subs = _repo.Payments.GetAllValidSubscriptions(source.Id);
            return subs.Any();
        }
    }
}
