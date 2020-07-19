using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Services;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileHasSubscribedResolver : IValueResolver<ApplicationUser, SubscriptionViewModel, bool> {

        private readonly IPaymentRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileHasSubscribedResolver(IPaymentRepository repository, UserManager<ApplicationUser> userManager) {
            _repository = repository;
            _userManager = userManager;
        }

        public bool Resolve(ApplicationUser source, SubscriptionViewModel destination, bool destMember, ResolutionContext context) {

            var isAdmin = AsyncHelper.RunSync(() => {
                return _userManager.IsInRoleAsync(source, "god-mode");
            });
            if (isAdmin) return true;

            var subs = _repository.GetAll()
                .Where(r => r.AppUser == source)
                .Count() != 0;
            return subs;
        }
    }
}
