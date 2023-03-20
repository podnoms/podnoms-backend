using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PodNoms.Data.Models;
using PodNoms.Common.Auth;

namespace PodNoms.Common.Persistence {
    public static class PodNomsDbInitialiser {
        public static void SeedPodcasts(
            UserManager<ApplicationUser> userManager,
            PodNomsDbContext context,
            IConfiguration config) {
            if (context.Podcasts.Any()) {
                return;
            }

            var testUser = config["AdminUserSettings:TestUser:Email"];
            var existing = userManager.FindByEmailAsync(testUser).Result;

            for (int i = 0; i < 100; i++) {
                var podcast = new Podcast {
                    Title = $"Test Podcast {i}",
                    Slug = $"test_podcast_{i}",
                    Category = context.Categories.FirstOrDefault(),
                    AppUserId = existing.Id,
                };
                context.Podcasts.Add(podcast);
            }

            context.SaveChanges();
        }

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
                new string[] {"catastrophic-api-calls-allowed", "website-admin", "god-mode"},
                userManager);

            var adminUser = _createUserIfNeeded(
                config["AdminUserSettings:AdminUser:UserName"],
                config["AdminUserSettings:AdminUser:Name"],
                config["AdminUserSettings:AdminUser:Email"],
                config["AdminUserSettings:AdminUser:Password"],
                new string[] {"website-admin"},
                userManager
            );
            if (adminUser == null) {
                return;
            }

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

        private static ApplicationUser _createUserIfNeeded(string userName, string name, string email, string password,
            string[] roles, UserManager<ApplicationUser> userManager) {
            var existing = userManager.FindBySlugAsync(userName).Result;
            if (existing != null) {
                return null;
            }

            var user = new ApplicationUser {
                UserName = email,
                FirstName = name,
                Email = email,
                Slug = userName
            };

            var result = userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded) {
                return user;
            }

            foreach (var role in roles) {
                userManager.AddToRoleAsync(user, role).Wait();
            }

            return user;
        }
    }
}
