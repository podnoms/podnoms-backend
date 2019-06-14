using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth {
    public static class UserManagerExtensions {
        public static async Task<ApplicationUser> FindBySlugAsync (this UserManager<ApplicationUser> userManager, string slug) {
            var user = await Task.Run (() => userManager.Users.SingleOrDefault (x => x.Slug == slug));
            return user;
        }
        public static async Task<bool> CheckSlug (this UserManager<ApplicationUser> userManager, string slug) {
            var user = await Task.Run (() => userManager.Users.SingleOrDefault (x => x.Slug == slug));
            return (user is null);
        }
    }
}
