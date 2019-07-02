using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class ProfileSubscriptionResolver : IValueResolver<ApplicationUser, ProfileViewModel, string> {
        public string Resolve(ApplicationUser source, ProfileViewModel destination, string destMember, ResolutionContext context) {
            return "TODO";
        }
    }
}
