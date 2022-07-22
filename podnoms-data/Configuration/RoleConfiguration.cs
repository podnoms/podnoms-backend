using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PodNoms.Data.Configuration {
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole> {
        public void Configure(EntityTypeBuilder<IdentityRole> builder) {
            builder.HasData(
                new IdentityRole {
                    Id = "94e79a7b-2ec7-46e2-b890-fcf0e5eaaf80",
                    Name = "client-admin",
                    NormalizedName = "client-admin".ToUpper(),
                    ConcurrencyStamp = "35eb0496-26cf-4ce9-b106-03c16d4566fb"
                },
                new IdentityRole {
                    Id = "f1c6e6a8-2461-48f2-ab7a-569a3b75b280",
                    Name = "website-admin",
                    NormalizedName = "website-admin".ToUpper(),
                    ConcurrencyStamp = "75cd9bcf-91ef-4819-b66c-0c22918ba6e8"
                },
                new IdentityRole {
                    Id = "dba18578-271a-40de-8cb3-e21f97fcf159",
                    Name = "catastrophic-api-calls-allowed",
                    NormalizedName = "catastrophic-api-calls-allowed".ToUpper(),
                    ConcurrencyStamp = "1d5a0bcd-6aff-4158-801d-d455b4ce6c25"
                },
                new IdentityRole {
                    Id = "9517774b-a900-49ee-9ddf-c31b36938352",
                    Name = "god-mode",
                    NormalizedName = "god-mode".ToUpper(),
                    ConcurrencyStamp = "ffbb9e22-e3a1-4631-ab85-239f26267de7"
                }
            );
        }
    }
}
