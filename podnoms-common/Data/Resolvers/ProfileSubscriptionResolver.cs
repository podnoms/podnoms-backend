using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Services;
using PodNoms.Data.Enums;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileSubscriptionResolver : IValueResolver<ApplicationUser, SubscriptionViewModel, string> {

        private readonly IPaymentRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileSubscriptionResolver(IPaymentRepository repository, UserManager<ApplicationUser> userManager) {
            _repository = repository;
            _userManager = userManager;
        }

        public string Resolve(ApplicationUser source, SubscriptionViewModel destination,
                                string destMember, ResolutionContext context) {
            var isAdmin = AsyncHelper.RunSync(() => _userManager.IsInRoleAsync(source, "god-mode"));
            
            return isAdmin ? AccountSubscriptionTier.VIP.ToString() : AccountSubscriptionTier.Freeloader.ToString();
        }
    }
}
