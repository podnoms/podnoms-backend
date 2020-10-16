using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodNoms.Data.Models;

namespace PodNoms.Data.Configuration {
    public class CategoryConfiguration : IEntityTypeConfiguration<Category> {
        public void Configure(EntityTypeBuilder<Category> builder) {
            builder.HasData(
                new Category {
                    Id = Guid.Parse("29C0716A-94BC-4B79-BB7A-1ACB2D872101"), Description = "Comedy",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                },
                new Category {
                    Id = Guid.Parse("DB829BFE-A8FE-458E-9B67-2B00F4794750"), Description = "Technology",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("50495352-2339-4498-AAD3-3F8C85F6AC69"), Description = "Science & Medicine",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Business",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Religion & Spirituality",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("3219621B-8311-4B65-BB48-6F68FBA4957C"), Description = "Kids & Family",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("A6AA8E20-8729-4698-A254-976012AFDBF3"), Description = "TV & Film",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("97735523-D87A-4B5F-9DD1-AB8289AF2AE6"), Description = "Sports & Recreation",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("F177D65B-5ECA-4137-B202-AF672CD11D70"), Description = "Society & Culture",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("27FB7005-B75C-490B-AE13-BCC88525BE65"), Description = "Music",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("C4911D87-2B6E-42EA-B771-BE910CB01624"), Description = "Health",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("5E023A7A-461D-46C6-BCA8-C9049F6D2EC5"), Description = "News & Politics",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("B13CDDB1-FEFF-42E1-9C80-CB8AD4A5F374"), Description = "Games & Hobbies",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "Education",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Arts",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }, new Category {
                    Id = Guid.Parse("2E23F263-062A-43C3-9E27-FB7555FB8E76"), Description = "Government & Organizations",
                    CreateDate = DateTime.MinValue, UpdateDate = DateTime.MinValue
                }
            );
        }
    }
}
