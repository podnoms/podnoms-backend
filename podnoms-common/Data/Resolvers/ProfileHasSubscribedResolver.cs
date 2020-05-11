using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class ProfileHasSubscribedResolver : IValueResolver<ApplicationUser, ProfileViewModel, bool> {

        private readonly IPaymentRepository _repository;

        public ProfileHasSubscribedResolver(IPaymentRepository repository) {
            _repository = repository;
        }

        public bool Resolve(ApplicationUser source, ProfileViewModel destination, bool destMember, ResolutionContext context) {
            var subs = _repository.GetAll()
                .Where(r => r.AppUser == source)
                .Count() != 0;
            return subs;
        }
    }
}
