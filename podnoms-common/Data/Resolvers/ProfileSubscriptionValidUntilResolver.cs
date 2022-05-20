using System;
using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class
        ProfileSubscriptionValidUntilResolver : IValueResolver<ApplicationUser, ProfileViewModel, DateTime?> {
        private readonly IRepoAccessor _repo;

        public ProfileSubscriptionValidUntilResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public DateTime? Resolve(ApplicationUser source, ProfileViewModel destination, DateTime? destMember,
            ResolutionContext context) {
            var latest = _repo.Payments.GetAll()
                .Where(r => r.AppUser == source)
                .Where(r => r.WasSuccessful)
                .OrderByDescending(r => r.EndDate)
                .FirstOrDefault();
            return latest?.EndDate;
        }
    }
}
