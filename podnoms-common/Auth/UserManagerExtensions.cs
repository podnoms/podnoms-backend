using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth {
    public static class UserManagerExtensions {
        public static async Task<ApplicationUser> FindBySlugAsync(this UserManager<ApplicationUser> userManager, string slug) {
            var user = await userManager.Users.SingleOrDefaultAsync(x => x.Slug == slug);
            return user;
        }
        public static async Task<ApplicationUser> FindByTwitterHandleAsync(this UserManager<ApplicationUser> userManager, string twitterHandle) {
            var user = await userManager.Users.SingleOrDefaultAsync(x => x.TwitterHandle == twitterHandle);
            return user;
        }
        public static async Task<bool> CheckSlug(this UserManager<ApplicationUser> userManager, string slug) {
            var user = await userManager.Users.SingleOrDefaultAsync(x => x.Slug == slug);
            return (user is null);
        }
    }
}
