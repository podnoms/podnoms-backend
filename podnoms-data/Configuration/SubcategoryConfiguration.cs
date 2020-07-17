using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodNoms.Data.Models;

namespace PodNoms.Data.Configuration {
    public class SubcategoryConfiguration : IEntityTypeConfiguration<Subcategory> {
        public void Configure(EntityTypeBuilder<Subcategory> builder) {
            builder.HasData(
                new Subcategory { Id = Guid.Parse("1F4B0B76-D7B8-404C-AF32-3428FC488E30"), CategoryId = Guid.Parse("B13CDDB1-FEFF-42E1-9C80-CB8AD4A5F374"), Description = "Video Games" },
                new Subcategory { Id = Guid.Parse("84ED0B38-616D-4926-90C2-3CC7AE2F8E4E"), CategoryId = Guid.Parse("97735523-D87A-4B5F-9DD1-AB8289AF2AE6"), Description = "Professional" },
                new Subcategory { Id = Guid.Parse("867AE49C-ADE1-41B9-BA18-3F4801EC18EE"), CategoryId = Guid.Parse("C4911D87-2B6E-42EA-B771-BE910CB01624"), Description = "Fitness & Nutrition" },
                new Subcategory { Id = Guid.Parse("B4284990-C542-48E4-A000-42D27202153B"), CategoryId = Guid.Parse("2E23F263-062A-43C3-9E27-FB7555FB8E76"), Description = "Non-Profit" },
                new Subcategory { Id = Guid.Parse("C950EAAC-F164-4F3C-8384-459679130AEF"), CategoryId = Guid.Parse("97735523-D87A-4B5F-9DD1-AB8289AF2AE6"), Description = "Outdoor" },
                new Subcategory { Id = Guid.Parse("DF0063A6-31EF-4F9A-BE23-4F9178291BB3"), CategoryId = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "Higher Education" },
                new Subcategory { Id = Guid.Parse("2488B878-124A-42F9-A761-53DAA1EEFDE1"), CategoryId = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Literature" },
                new Subcategory { Id = Guid.Parse("5C40FA5A-72DF-475B-BFA8-561955145762"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Other" },
                new Subcategory { Id = Guid.Parse("201FB42B-E241-4E16-BEA5-5DF768064402"), CategoryId = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Investing" },
                new Subcategory { Id = Guid.Parse("4F006295-E8EA-4A87-92E0-69E32F8D3AD9"), CategoryId = Guid.Parse("B13CDDB1-FEFF-42E1-9C80-CB8AD4A5F374"), Description = "Aviation" },
                new Subcategory { Id = Guid.Parse("8B2A590B-BB59-4F30-ACB2-72FA01985E4B"), CategoryId = Guid.Parse("50495352-2339-4498-AAD3-3F8C85F6AC69"), Description = "Social Sciences" },
                new Subcategory { Id = Guid.Parse("3E3F77A4-4CD3-4644-ACE1-7923E84D403D"), CategoryId = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Performing Arts" },
                new Subcategory { Id = Guid.Parse("F9167EDB-5D9C-4E34-AD26-7A2630528682"), CategoryId = Guid.Parse("F177D65B-5ECA-4137-B202-AF672CD11D70"), Description = "Places & Travel" },
                new Subcategory { Id = Guid.Parse("4ECD4DB0-0786-4594-A49B-86AB1362BC3C"), CategoryId = Guid.Parse("97735523-D87A-4B5F-9DD1-AB8289AF2AE6"), Description = "Amateur" },
                new Subcategory { Id = Guid.Parse("375FA60D-8D6F-4684-A729-8A061CE2E062"), CategoryId = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Management & Marketing" },
                new Subcategory { Id = Guid.Parse("5005A13D-45C9-40EA-B691-96FF5AFA0E39"), CategoryId = Guid.Parse("F177D65B-5ECA-4137-B202-AF672CD11D70"), Description = "Personal Journals" },
                new Subcategory { Id = Guid.Parse("D9D8F925-CF6F-4D8D-9404-990609B312CE"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Buddhism" },
                new Subcategory { Id = Guid.Parse("D5E38344-C701-406F-8762-9A793EFB98D7"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Christianity" },
                new Subcategory { Id = Guid.Parse("BD367623-40C3-48F2-9A59-9B526C643905"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Hinduism" },
                new Subcategory { Id = Guid.Parse("797408D2-ABBC-4D8D-92F4-9B74146D726D"), CategoryId = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Business News" },
                new Subcategory { Id = Guid.Parse("7174AA7C-8DF2-4F9F-84AA-9BC1E9793EDA"), CategoryId = Guid.Parse("B13CDDB1-FEFF-42E1-9C80-CB8AD4A5F374"), Description = "Other Games" },
                new Subcategory { Id = Guid.Parse("D69E2CC2-588F-49C5-933E-A6F963E79F32"), CategoryId = Guid.Parse("2E23F263-062A-43C3-9E27-FB7555FB8E76"), Description = "Local" },
                new Subcategory { Id = Guid.Parse("C8829517-5A62-4BCC-82CC-AB4A75C6312C"), CategoryId = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "K-12" },
                new Subcategory { Id = Guid.Parse("7BE75C87-D18A-45EE-92C3-AB6AFCE3E5DB"), CategoryId = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Food" },
                new Subcategory { Id = Guid.Parse("1D5212BC-B31C-4130-BCEA-AE7C9DF2985F"), CategoryId = Guid.Parse("F177D65B-5ECA-4137-B202-AF672CD11D70"), Description = "Philosophy" },
                new Subcategory { Id = Guid.Parse("4966EC0C-3F53-4710-9F21-AFB2371AB3E2"), CategoryId = Guid.Parse("C4911D87-2B6E-42EA-B771-BE910CB01624"), Description = "Sexuality" },
                new Subcategory { Id = Guid.Parse("D8C5C265-1FA1-4244-9D76-B181BD936846"), CategoryId = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "Education Technology" },
                new Subcategory { Id = Guid.Parse("7E371AB3-CC53-4F16-9DE9-C2D10FAA8938"), CategoryId = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Visual Arts" },
                new Subcategory { Id = Guid.Parse("C546F84F-643A-4F44-9BDF-CE4B44926E0A"), CategoryId = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "Training" },
                new Subcategory { Id = Guid.Parse("149F1619-EB42-4D9B-A4D6-DA68EC0541D9"), CategoryId = Guid.Parse("DB829BFE-A8FE-458E-9B67-2B00F4794750"), Description = "Podcasting" },
                new Subcategory { Id = Guid.Parse("37F4F907-7B02-4268-8E38-DB8700C1DC87"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Judaism" },
                new Subcategory { Id = Guid.Parse("8114A581-24BB-451E-BA21-DCACD175BDE4"), CategoryId = Guid.Parse("AD31686B-794B-4EBB-99A8-CDC812CA7E83"), Description = "Language Courses" },
                new Subcategory { Id = Guid.Parse("35CF95E2-6506-47AD-BD5D-DF3B5E389A8F"), CategoryId = Guid.Parse("41B9EE87-A9CA-4305-8ED8-EE69A3DBCFC3"), Description = "Fashion & Beauty" },
                new Subcategory { Id = Guid.Parse("8FB81212-5911-4190-9635-E478E2119C0C"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Spirituality" },
                new Subcategory { Id = Guid.Parse("23EB4B47-FC1D-45AC-9A21-E4EA5313F3D8"), CategoryId = Guid.Parse("B13CDDB1-FEFF-42E1-9C80-CB8AD4A5F374"), Description = "Hobbies" },
                new Subcategory { Id = Guid.Parse("C7465257-D5B6-46F3-8521-E7A8F91E17B8"), CategoryId = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Shopping" },
                new Subcategory { Id = Guid.Parse("CBCF3C5F-F7C9-44B3-9B2D-EDC6523D8C3C"), CategoryId = Guid.Parse("C4911D87-2B6E-42EA-B771-BE910CB01624"), Description = "Self-Help" },
                new Subcategory { Id = Guid.Parse("FB7C2755-4B09-4D9F-A457-EEBB39ABAC3B"), CategoryId = Guid.Parse("F0603194-6F45-4695-98C1-6288CFFBFD94"), Description = "Careers" },
                new Subcategory { Id = Guid.Parse("EB082AC8-856B-4BB0-A22F-F4062D4347D2"), CategoryId = Guid.Parse("DB829BFE-A8FE-458E-9B67-2B00F4794750"), Description = "Software How-To" },
                new Subcategory { Id = Guid.Parse("56A5DC3B-3DC5-46D2-9972-F846A4CA9909"), CategoryId = Guid.Parse("67D057A7-21B4-4462-A284-66BA62A6DE1B"), Description = "Islam" }
            );
        }
    }
}
