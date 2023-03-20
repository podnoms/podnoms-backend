using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Services;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileSubscriptionResolver : IValueResolver<ApplicationUser, SubscriptionViewModel, string> {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileSubscriptionResolver(UserManager<ApplicationUser> userManager) {
            _userManager = userManager;
        }

        public string Resolve(ApplicationUser source, SubscriptionViewModel destination,
            string destMember, ResolutionContext context) {
            var isAdmin = AsyncHelper.RunSync(() => _userManager.IsInRoleAsync(source, "god-mode"));

            return isAdmin ? AccountSubscriptionTier.VIP.ToString() : AccountSubscriptionTier.Freeloader.ToString();
        }
    }
}
