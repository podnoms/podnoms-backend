using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence {
    public static class PodNomsDbInitialiser {
        public static void SeedUsers(
                    UserManager<ApplicationUser> userManager,
                    PodNomsDbContext context,
                    IConfiguration config) {
            var item = config["AdminUserSettings"];
            var nestedItem = config["AdminUserSettings"];
            var user = _createUserIfNeeded(
                 config["AdminUserSettings:TestUser:UserName"],
                 config["AdminUserSettings:TestUser:Name"],
                 config["AdminUserSettings:TestUser:Email"],
                 config["AdminUserSettings:TestUser:Password"],
                 new string[] { "catastrophic-api-calls-allowed", "website-admin", "god-mode" },
                 userManager);

            var adminUser = _createUserIfNeeded(
                config["AdminUserSettings:AdminUser:UserName"],
                config["AdminUserSettings:AdminUser:Name"],
                config["AdminUserSettings:AdminUser:Email"],
                config["AdminUserSettings:AdminUser:Password"],
                new string[] { "website-admin" },
                userManager
            );
            if (adminUser != null) {
                var sql = @$"INSERT INTO dbo.IssuedApiKeys
                    (
                        Id,
                        CreateDate,
                        UpdateDate,
                        Name,
                        Prefix,
                        [Key],
                        Scopes,
                        IsValid,
                        Expires,
                        IssuedToId
                    )
                    VALUES
                    (   NEWID(),
                        SYSDATETIME(),
                        SYSDATETIME(),
                        N'{config["AdminUserSettings:AdminUser:ApiKeyName"]}',
                        N'{config["AdminUserSettings:AdminUser:ApiKeyPrefix"]}',
                        N'{config["AdminUserSettings: AdminUser:ApiKey"]}',
                        NULL,
                        1,
                        SYSDATETIME(),
                        N'{adminUser.Id}'
                        )";
                context.Database.ExecuteSqlRaw(sql);
            }
        }

        private static ApplicationUser _createUserIfNeeded(string userName, string name, string email, string password, string[] roles, UserManager<ApplicationUser> userManager) {
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
                return user;
            }
            return null;
        }
    }
}
