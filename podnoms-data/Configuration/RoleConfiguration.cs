using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PodNoms.Data.Configuration {
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole> {
        public void Configure(EntityTypeBuilder<IdentityRole> builder) {
            builder.HasData(
                new IdentityRole {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = "website-admin",
                    NormalizedName = "website-admin".ToUpper()
                },
                new IdentityRole {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = "catastrophic-api-calls-allowed",
                    NormalizedName = "catastrophic-api-calls-allowed".ToUpper()
                }
            );
        }
    }
}
