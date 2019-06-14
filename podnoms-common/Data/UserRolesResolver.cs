using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class UserRolesResolver : IValueResolver<ApplicationUser, ProfileViewModel, List<string>> {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRolesResolver (UserManager<ApplicationUser> userManager) {
            _userManager = userManager;
        }

        public List<string> Resolve (ApplicationUser source, ProfileViewModel destination, List<string> destMember, ResolutionContext context) {
            var t = _userManager.GetRolesAsync (source);
            Task.WhenAll (t);
            return t.Result.ToList ();
        }
    }
}
