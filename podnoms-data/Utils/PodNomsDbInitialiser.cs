using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PodNoms.Data.Models;

namespace PodNoms.Data.Utils {
    public static class PodNomsDbInitialiser {
        public static void SeedUsers(UserManager<ApplicationUser> userManager, IConfiguration config) {
            var item = config["AdminUserSettings"];
            var nestedItem = config["AdminUserSettings"];
            _createUserIfNeeded(
                config["AdminUserSettings:TestUser:UserName"],
                config["AdminUserSettings:TestUser:Name"],
                config["AdminUserSettings:TestUser:Email"],
                config["AdminUserSettings:TestUser:Password"],
                new string[] { "catastrophic-api-calls-allowed" },
                userManager
            );
            _createUserIfNeeded(
                config["AdminUserSettings:AdminUser:UserName"],
                config["AdminUserSettings:AdminUser:Name"],
                config["AdminUserSettings:AdminUser:Email"],
                config["AdminUserSettings:AdminUser:Password"],
                new string[] { "website-admin" },
                userManager
            );
        }

        private static void _createUserIfNeeded(string userName, string name, string email, string password, string[] roles, UserManager<ApplicationUser> userManager) {
            if (userManager.FindByEmailAsync(email).Result == null) {
                var user = new ApplicationUser {
                    UserName = email,
                    FirstName = name,
                    Email = email,
                    Slug = userName
                };

                var result = userManager.CreateAsync(user, password).Result;
                if (result.Succeeded) {
                    foreach (var role in roles) {
                        userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
        }
    }
}
