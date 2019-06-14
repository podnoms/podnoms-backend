using System;
using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class ProfileSubscriptionValidUntilResolver : IValueResolver<ApplicationUser, ProfileViewModel, DateTime?> {
        private readonly IPaymentRepository _repository;

        public ProfileSubscriptionValidUntilResolver (IPaymentRepository repository) {
            _repository = repository;
        }
        public DateTime? Resolve (ApplicationUser source, ProfileViewModel destination, DateTime? destMember, ResolutionContext context) {
            var latest = _repository.GetAll ()
                .Where (r => r.AppUser == source)
                .Where (r => r.WasSuccessful)
                .OrderByDescending (r => r.EndDate)
                .FirstOrDefault ();
            return latest?.EndDate;
        }
    }
}
