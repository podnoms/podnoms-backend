using System;
using System.Linq;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;
using PodNoms.Data.Extensions;

namespace PodNoms.Common.Data {
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
            Console.WriteLine(subs.AsQueryable().ToSql());
            return subs.Count() > 0;
        }
    }
}