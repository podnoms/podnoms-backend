using System.Collections.Generic;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Data {
    internal class ProfileSubscriptionTypeResolver : IValueResolver<ApplicationUser, ProfileViewModel, string> {
        public string Resolve(ApplicationUser source, ProfileViewModel destination, string destMember, ResolutionContext context) {
            return "TODO";
        }
    }
}