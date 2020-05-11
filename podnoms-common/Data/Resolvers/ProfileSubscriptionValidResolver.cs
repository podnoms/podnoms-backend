using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;
using System.Linq;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileSubscriptionValidResolver : IValueResolver<ApplicationUser, ProfileViewModel, bool> {
        private readonly IPaymentRepository _repository;

        public ProfileSubscriptionValidResolver(IPaymentRepository repository) {
            _repository = repository;
        }

        public bool Resolve(ApplicationUser source, ProfileViewModel destination, bool destMember, ResolutionContext context) {
            var test = _repository.GetAll()
                .Where(r => r.AppUser == source)
                .FirstOrDefault();

            var subs = _repository.GetAllValidSubscriptions(source.Id);
            return subs.Count() > 0;
        }
    }
}
