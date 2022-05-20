using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileHasSubscribedResolver : IValueResolver<ApplicationUser, SubscriptionViewModel, bool> {
        private readonly IRepoAccessor _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileHasSubscribedResolver(IRepoAccessor repo, UserManager<ApplicationUser> userManager) {
            _repo = repo;
            _userManager = userManager;
        }

        public bool Resolve(ApplicationUser source, SubscriptionViewModel destination, bool destMember,
            ResolutionContext context) {
            var isAdmin = AsyncHelper.RunSync(() => _userManager.IsInRoleAsync(source, "god-mode"));
            if (isAdmin) return true;

            var subs = _repo.Payments
                .GetAll()
                .Any(r => r.AppUser == source);
            return subs;
        }
    }
}
