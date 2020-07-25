using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class AddRoleAndCategorySeeds : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "f1c6e6a8-2461-48f2-ab7a-569a3b75b280", "5bbeb1b1-09b6-45ab-8fd2-7131445bb72b", "website-admin", "WEBSITE-ADMIN" },
                    { "6105993d-c572-4c5c-9c9f-acc1a031e4f3", "ddba6285-ea21-453c-b4c8-d381c118603e",  "god-mode", "GOD-MODE"},
                    { "dba18578-271a-40de-8cb3-e21f97fcf159", "da355d84-9e05-4c71-8356-cc433ca4e42c", "catastrophic-api-calls-allowed", "CATASTROPHIC-API-CALLS-ALLOWED" }
                });

            migrationBuilder.Sql("ALTER TABLE dbo.Podcasts NOCHECK CONSTRAINT ALL");
            migrationBuilder.Sql("DELETE FROM dbo.Categories");
            // migrationBuilder.Sql("ALTER TABLE dbo.Categories NOCHECK CONSTRAINT [PK_Categories]");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreateDate", "Description" },
                values: new object[,]
                {
                    { new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1152), "Education" },
                    { new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1149), "Games & Hobbies" },
                    { new Guid("5e023a7a-461d-46c6-bca8-c9049f6d2ec5"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1147), "News & Politics" },
                    { new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1145), "Health" },
                    { new Guid("27fb7005-b75c-490b-ae13-bcc88525be65"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1142), "Music" },
                    { new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1140), "Society & Culture" },
                    { new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1138), "Sports & Recreation" },
                    { new Guid("a6aa8e20-8729-4698-a254-976012afdbf3"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1135), "TV & Film" },
                    { new Guid("3219621b-8311-4b65-bb48-6f68fba4957c"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1132), "Kids & Family" },
                    { new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1130), "Religion & Spirituality" },
                    { new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1127), "Business" },
                    { new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1124), "Science & Medicine" },
                    { new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1103), "Technology" },
                    { new Guid("29c0716a-94bc-4b79-bb7a-1acb2d872101"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(606), "Comedy" },
                    { new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1154), "Arts" },
                    { new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), new DateTime(2020, 7, 17, 2, 56, 39, 448, DateTimeKind.Utc).AddTicks(1156), "Government & Organizations" }
                });

            migrationBuilder.InsertData(
                table: "Subcategories",
                columns: new[] { "Id", "CategoryId", "CreateDate", "Description", "PodcastId" },
                values: new object[,]
                {
                    { new Guid("149f1619-eb42-4d9b-a4d6-da68ec0541d9"), new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4296), "Podcasting", null },
                    { new Guid("4966ec0c-3f53-4710-9f21-afb2371ab3e2"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4281), "Sexuality", null },
                    { new Guid("cbcf3c5f-f7c9-44b3-9b2d-edc6523d8c3c"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4322), "Self-Help", null },
                    { new Guid("1f4b0b76-d7b8-404c-af32-3428fc488e30"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(3775), "Video Games", null },
                    { new Guid("4f006295-e8ea-4a87-92e0-69e32f8d3ad9"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4223), "Aviation", null },
                    { new Guid("7174aa7c-8df2-4f9f-84aa-9bc1e9793eda"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4263), "Other Games", null },
                    { new Guid("23eb4b47-fc1d-45ac-9a21-e4ea5313f3d8"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4315), "Hobbies", null },
                    { new Guid("df0063a6-31ef-4f9a-be23-4f9178291bb3"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4208), "Higher Education", null },
                    { new Guid("867ae49c-ade1-41b9-ba18-3f4801ec18ee"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4195), "Fitness & Nutrition", null },
                    { new Guid("c8829517-5a62-4bcc-82cc-ab4a75c6312c"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4270), "K-12", null },
                    { new Guid("c546f84f-643a-4f44-9bdf-ce4b44926e0a"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4293), "Training", null },
                    { new Guid("8114a581-24bb-451e-ba21-dcacd175bde4"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4304), "Language Courses", null },
                    { new Guid("2488b878-124a-42f9-a761-53daa1eefde1"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4212), "Literature", null },
                    { new Guid("3e3f77a4-4cd3-4644-ace1-7923e84d403d"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4230), "Performing Arts", null },
                    { new Guid("7be75c87-d18a-45ee-92c3-ab6afce3e5db"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4274), "Food", null },
                    { new Guid("7e371ab3-cc53-4f16-9de9-c2d10faa8938"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4289), "Visual Arts", null },
                    { new Guid("35cf95e2-6506-47ad-bd5d-df3b5e389a8f"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4308), "Fashion & Beauty", null },
                    { new Guid("d8c5c265-1fa1-4244-9d76-b181bd936846"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4285), "Education Technology", null },
                    { new Guid("1d5212bc-b31c-4130-bcea-ae7c9df2985f"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4277), "Philosophy", null },
                    { new Guid("5005a13d-45c9-40ea-b691-96ff5afa0e39"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4244), "Personal Journals", null },
                    { new Guid("f9167edb-5d9c-4e34-ad26-7a2630528682"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4233), "Places & Travel", null },
                    { new Guid("eb082ac8-856b-4bb0-a22f-f4062d4347d2"), new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4330), "Software How-To", null },
                    { new Guid("8b2a590b-bb59-4f30-acb2-72fa01985e4b"), new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4226), "Social Sciences", null },
                    { new Guid("201fb42b-e241-4e16-bea5-5df768064402"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4219), "Investing", null },
                    { new Guid("375fa60d-8d6f-4684-a729-8a061ce2e062"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4240), "Management & Marketing", null },
                    { new Guid("797408d2-abbc-4d8d-92f4-9b74146d726d"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4259), "Business News", null },
                    { new Guid("c7465257-d5b6-46f3-8521-e7a8f91e17b8"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4319), "Shopping", null },
                    { new Guid("fb7c2755-4b09-4d9f-a457-eebb39abac3b"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4326), "Careers", null },
                    { new Guid("5c40fa5a-72df-475b-bfa8-561955145762"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4215), "Other", null },
                    { new Guid("d9d8f925-cf6f-4d8d-9404-990609b312ce"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4248), "Buddhism", null },
                    { new Guid("d5e38344-c701-406f-8762-9a793efb98d7"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4251), "Christianity", null },
                    { new Guid("bd367623-40c3-48f2-9a59-9b526c643905"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4255), "Hinduism", null },
                    { new Guid("37f4f907-7b02-4268-8e38-db8700c1dc87"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4300), "Judaism", null },
                    { new Guid("8fb81212-5911-4190-9635-e478e2119c0c"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4312), "Spirituality", null },
                    { new Guid("56a5dc3b-3dc5-46d2-9972-f846a4ca9909"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4334), "Islam", null },
                    { new Guid("84ed0b38-616d-4926-90c2-3cc7ae2f8e4e"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4176), "Professional", null },
                    { new Guid("c950eaac-f164-4f3c-8384-459679130aef"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4204), "Outdoor", null },
                    { new Guid("4ecd4db0-0786-4594-a49b-86ab1362bc3c"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4237), "Amateur", null },
                    { new Guid("b4284990-c542-48e4-a000-42d27202153b"), new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4200), "Non-Profit", null },
                    { new Guid("d69e2cc2-588f-49c5-933e-a6f963e79f32"), new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), new DateTime(2020, 7, 17, 2, 56, 39, 449, DateTimeKind.Utc).AddTicks(4266), "Local", null }
                });
            migrationBuilder.Sql("ALTER TABLE dbo.Podcasts CHECK CONSTRAINT ALL");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dba18578-271a-40de-8cb3-e21f97fcf159");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f1c6e6a8-2461-48f2-ab7a-569a3b75b280");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("27fb7005-b75c-490b-ae13-bcc88525be65"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("29c0716a-94bc-4b79-bb7a-1acb2d872101"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3219621b-8311-4b65-bb48-6f68fba4957c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5e023a7a-461d-46c6-bca8-c9049f6d2ec5"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a6aa8e20-8729-4698-a254-976012afdbf3"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("149f1619-eb42-4d9b-a4d6-da68ec0541d9"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1d5212bc-b31c-4130-bcea-ae7c9df2985f"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1f4b0b76-d7b8-404c-af32-3428fc488e30"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("201fb42b-e241-4e16-bea5-5df768064402"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("23eb4b47-fc1d-45ac-9a21-e4ea5313f3d8"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("2488b878-124a-42f9-a761-53daa1eefde1"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("35cf95e2-6506-47ad-bd5d-df3b5e389a8f"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("375fa60d-8d6f-4684-a729-8a061ce2e062"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("37f4f907-7b02-4268-8e38-db8700c1dc87"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("3e3f77a4-4cd3-4644-ace1-7923e84d403d"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4966ec0c-3f53-4710-9f21-afb2371ab3e2"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4ecd4db0-0786-4594-a49b-86ab1362bc3c"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4f006295-e8ea-4a87-92e0-69e32f8d3ad9"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5005a13d-45c9-40ea-b691-96ff5afa0e39"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("56a5dc3b-3dc5-46d2-9972-f846a4ca9909"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5c40fa5a-72df-475b-bfa8-561955145762"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7174aa7c-8df2-4f9f-84aa-9bc1e9793eda"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("797408d2-abbc-4d8d-92f4-9b74146d726d"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7be75c87-d18a-45ee-92c3-ab6afce3e5db"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7e371ab3-cc53-4f16-9de9-c2d10faa8938"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8114a581-24bb-451e-ba21-dcacd175bde4"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("84ed0b38-616d-4926-90c2-3cc7ae2f8e4e"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("867ae49c-ade1-41b9-ba18-3f4801ec18ee"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8b2a590b-bb59-4f30-acb2-72fa01985e4b"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8fb81212-5911-4190-9635-e478e2119c0c"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("b4284990-c542-48e4-a000-42d27202153b"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("bd367623-40c3-48f2-9a59-9b526c643905"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c546f84f-643a-4f44-9bdf-ce4b44926e0a"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c7465257-d5b6-46f3-8521-e7a8f91e17b8"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c8829517-5a62-4bcc-82cc-ab4a75c6312c"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c950eaac-f164-4f3c-8384-459679130aef"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("cbcf3c5f-f7c9-44b3-9b2d-edc6523d8c3c"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d5e38344-c701-406f-8762-9a793efb98d7"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d69e2cc2-588f-49c5-933e-a6f963e79f32"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d8c5c265-1fa1-4244-9d76-b181bd936846"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d9d8f925-cf6f-4d8d-9404-990609b312ce"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("df0063a6-31ef-4f9a-be23-4f9178291bb3"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("eb082ac8-856b-4bb0-a22f-f4062d4347d2"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("f9167edb-5d9c-4e34-ad26-7a2630528682"));

            migrationBuilder.DeleteData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("fb7c2755-4b09-4d9f-a457-eebb39abac3b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f177d65b-5eca-4137-b202-af672cd11d70"));
        }
    }
}
