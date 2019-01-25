using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class ProfileHasSubscribedTypeResolver : IValueResolver<ApplicationUser, ProfileViewModel, bool> {
        public bool Resolve(ApplicationUser source, ProfileViewModel destination, bool destMember, ResolutionContext context) {
            var subs = source.AccountSubscriptions.Count != 0;
            return subs;
        }
    }
}