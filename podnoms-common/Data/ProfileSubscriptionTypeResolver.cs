using System;
using System.Collections.Generic;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Data {
    internal class ProfileSubscriptionTypeResolver : IMemberValueResolver<ApplicationUser, ProfileViewModel, string,
        string> {
        public string Resolve(ApplicationUser source, ProfileViewModel destination, string sourceMember,
            string destMember,
            ResolutionContext context) {
            return "TODO";
        }
    }

//
//    internal class ProfileSubscriptionValidUntilResolver : IMemberValueResolver<ApplicationUser, ProfileViewModel,
//        DateTime,
//        DateTime> {
//        public string Resolve(ApplicationUser source, ProfileViewModel destination, DateTime sourceMember,
//            DateTime destMember,
//            ResolutionContext context) {
//            throw new System.NotImplementedException();
//        }
//    }
}