using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PodNoms.Identity.Data;

public class PodnomsAuthDbContext : IdentityDbContext<ApplicationUser> {
    public PodnomsAuthDbContext(DbContextOptions<PodnomsAuthDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("user_roles").HasKey(k => new {k.RoleId, k.UserId});
        builder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("user_logins").HasKey(k => new {k.UserId, k.LoginProvider});
        builder.Entity<IdentityRoleClaim<int>>().ToTable("role_claim");
        builder.Entity<IdentityUserToken<int>>().ToTable("user_token").HasKey(k => new {k.UserId, k.LoginProvider});
    }
}
