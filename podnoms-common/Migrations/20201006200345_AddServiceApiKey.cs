using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class AddServiceApiKey : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "ServicesApiKeys",
                columns: table => new {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreateDate =
                        table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate =
                        table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table => {
                    table.PrimaryKey("PK_ServicesApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicesApiKeyLogs",
                columns: table => new {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stack = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate =
                        table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate =
                        table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table => {
                    table.PrimaryKey("PK_ServicesApiKeyLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesApiKeyLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicesApiKeyLogs_ServicesApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ServicesApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("27fb7005-b75c-490b-ae13-bcc88525be65"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6004));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("29c0716a-94bc-4b79-bb7a-1acb2d872101"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5026));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6023));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3219621b-8311-4b65-bb48-6f68fba4957c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5991));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6020));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5981));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5e023a7a-461d-46c6-bca8-c9049f6d2ec5"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6010));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5988));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5998));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a6aa8e20-8729-4698-a254-976012afdbf3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5994));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6016));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6013));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6007));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5970));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(5985));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f177d65b-5eca-4137-b202-af672cd11d70"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 343, DateTimeKind.Utc).AddTicks(6001));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("149f1619-eb42-4d9b-a4d6-da68ec0541d9"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9305));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1d5212bc-b31c-4130-bcea-ae7c9df2985f"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9216));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1f4b0b76-d7b8-404c-af32-3428fc488e30"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(8662));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("201fb42b-e241-4e16-bea5-5df768064402"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9163));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("23eb4b47-fc1d-45ac-9a21-e4ea5313f3d8"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9322));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("2488b878-124a-42f9-a761-53daa1eefde1"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9156));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("35cf95e2-6506-47ad-bd5d-df3b5e389a8f"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9315));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("375fa60d-8d6f-4684-a729-8a061ce2e062"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9183));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("37f4f907-7b02-4268-8e38-db8700c1dc87"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9308));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("3e3f77a4-4cd3-4644-ace1-7923e84d403d"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9173));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4966ec0c-3f53-4710-9f21-afb2371ab3e2"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9219));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4ecd4db0-0786-4594-a49b-86ab1362bc3c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9179));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4f006295-e8ea-4a87-92e0-69e32f8d3ad9"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9166));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5005a13d-45c9-40ea-b691-96ff5afa0e39"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9186));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("56a5dc3b-3dc5-46d2-9972-f846a4ca9909"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9338));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5c40fa5a-72df-475b-bfa8-561955145762"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9159));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7174aa7c-8df2-4f9f-84aa-9bc1e9793eda"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9202));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("797408d2-abbc-4d8d-92f4-9b74146d726d"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9199));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7be75c87-d18a-45ee-92c3-ab6afce3e5db"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9212));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7e371ab3-cc53-4f16-9de9-c2d10faa8938"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9225));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8114a581-24bb-451e-ba21-dcacd175bde4"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9312));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("84ed0b38-616d-4926-90c2-3cc7ae2f8e4e"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9132));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("867ae49c-ade1-41b9-ba18-3f4801ec18ee"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9141));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8b2a590b-bb59-4f30-acb2-72fa01985e4b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9169));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8fb81212-5911-4190-9635-e478e2119c0c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9319));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("b4284990-c542-48e4-a000-42d27202153b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9145));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("bd367623-40c3-48f2-9a59-9b526c643905"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9196));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c546f84f-643a-4f44-9bdf-ce4b44926e0a"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9301));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c7465257-d5b6-46f3-8521-e7a8f91e17b8"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9325));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c8829517-5a62-4bcc-82cc-ab4a75c6312c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9209));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c950eaac-f164-4f3c-8384-459679130aef"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9149));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("cbcf3c5f-f7c9-44b3-9b2d-edc6523d8c3c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9328));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d5e38344-c701-406f-8762-9a793efb98d7"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9192));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d69e2cc2-588f-49c5-933e-a6f963e79f32"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9205));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d8c5c265-1fa1-4244-9d76-b181bd936846"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9222));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d9d8f925-cf6f-4d8d-9404-990609b312ce"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9189));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("df0063a6-31ef-4f9a-be23-4f9178291bb3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9153));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("eb082ac8-856b-4bb0-a22f-f4062d4347d2"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9335));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("f9167edb-5d9c-4e34-ad26-7a2630528682"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9176));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("fb7c2755-4b09-4d9f-a457-eebb39abac3b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 20, 3, 45, 344, DateTimeKind.Utc).AddTicks(9331));

            migrationBuilder.CreateIndex(
                name: "IX_ServicesApiKeyLogs_ApiKeyId",
                table: "ServicesApiKeyLogs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesApiKeyLogs_UserId",
                table: "ServicesApiKeyLogs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ServicesApiKeyLogs");

            migrationBuilder.DropTable(
                name: "ServicesApiKeys");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b00fda1d-656d-4597-96ef-d390d5e720d5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e05a2baf-abac-479a-a187-dee69c4eece7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f16a6986-478e-4d24-b995-1e86e3571b60");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] {"Id", "ConcurrencyStamp", "Name", "NormalizedName"},
                values: new object[,] {
                    {
                        "81ae1237-a3d8-4bc9-b9f2-6bce5e44b00f", "1376ab56-7748-477c-ba73-e68fea4be1f6", "website-admin",
                        "WEBSITE-ADMIN"
                    }, {
                        "6647ee71-5c01-4017-a4e9-b362a3bf0fe7", "eefd214c-35b6-4c65-8066-05523e4560e9",
                        "catastrophic-api-calls-allowed", "CATASTROPHIC-API-CALLS-ALLOWED"
                    }, {
                        "188f244e-8aa3-4f69-8582-f2c123c31159", "0a269916-32ec-4860-82bf-c6ebb6cb7f08", "god-mode",
                        "GOD-MODE"
                    }
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("27fb7005-b75c-490b-ae13-bcc88525be65"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9351));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("29c0716a-94bc-4b79-bb7a-1acb2d872101"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(8817));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9364));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3219621b-8311-4b65-bb48-6f68fba4957c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9341));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9362));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9334));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5e023a7a-461d-46c6-bca8-c9049f6d2ec5"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9355));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9339));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9346));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a6aa8e20-8729-4698-a254-976012afdbf3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9344));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9360));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9357));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9353));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9328));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9337));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f177d65b-5eca-4137-b202-af672cd11d70"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 693, DateTimeKind.Utc).AddTicks(9348));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("149f1619-eb42-4d9b-a4d6-da68ec0541d9"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(561));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1d5212bc-b31c-4130-bcea-ae7c9df2985f"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(544));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("1f4b0b76-d7b8-404c-af32-3428fc488e30"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(35));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("201fb42b-e241-4e16-bea5-5df768064402"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(491));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("23eb4b47-fc1d-45ac-9a21-e4ea5313f3d8"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(578));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("2488b878-124a-42f9-a761-53daa1eefde1"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(484));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("35cf95e2-6506-47ad-bd5d-df3b5e389a8f"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(571));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("375fa60d-8d6f-4684-a729-8a061ce2e062"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(511));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("37f4f907-7b02-4268-8e38-db8700c1dc87"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(564));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("3e3f77a4-4cd3-4644-ace1-7923e84d403d"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(501));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4966ec0c-3f53-4710-9f21-afb2371ab3e2"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(548));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4ecd4db0-0786-4594-a49b-86ab1362bc3c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(507));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("4f006295-e8ea-4a87-92e0-69e32f8d3ad9"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(494));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5005a13d-45c9-40ea-b691-96ff5afa0e39"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(514));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("56a5dc3b-3dc5-46d2-9972-f846a4ca9909"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(594));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("5c40fa5a-72df-475b-bfa8-561955145762"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(488));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7174aa7c-8df2-4f9f-84aa-9bc1e9793eda"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(531));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("797408d2-abbc-4d8d-92f4-9b74146d726d"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(528));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7be75c87-d18a-45ee-92c3-ab6afce3e5db"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(541));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("7e371ab3-cc53-4f16-9de9-c2d10faa8938"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(554));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8114a581-24bb-451e-ba21-dcacd175bde4"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(568));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("84ed0b38-616d-4926-90c2-3cc7ae2f8e4e"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(462));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("867ae49c-ade1-41b9-ba18-3f4801ec18ee"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(469));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8b2a590b-bb59-4f30-acb2-72fa01985e4b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(497));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("8fb81212-5911-4190-9635-e478e2119c0c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(575));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("b4284990-c542-48e4-a000-42d27202153b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(473));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("bd367623-40c3-48f2-9a59-9b526c643905"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(524));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c546f84f-643a-4f44-9bdf-ce4b44926e0a"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(558));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c7465257-d5b6-46f3-8521-e7a8f91e17b8"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(581));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c8829517-5a62-4bcc-82cc-ab4a75c6312c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(538));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("c950eaac-f164-4f3c-8384-459679130aef"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(477));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("cbcf3c5f-f7c9-44b3-9b2d-edc6523d8c3c"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(585));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d5e38344-c701-406f-8762-9a793efb98d7"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(521));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d69e2cc2-588f-49c5-933e-a6f963e79f32"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(534));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d8c5c265-1fa1-4244-9d76-b181bd936846"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(551));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("d9d8f925-cf6f-4d8d-9404-990609b312ce"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(517));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("df0063a6-31ef-4f9a-be23-4f9178291bb3"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(480));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("eb082ac8-856b-4bb0-a22f-f4062d4347d2"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(591));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("f9167edb-5d9c-4e34-ad26-7a2630528682"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(504));

            migrationBuilder.UpdateData(
                table: "Subcategories",
                keyColumn: "Id",
                keyValue: new Guid("fb7c2755-4b09-4d9f-a457-eebb39abac3b"),
                column: "CreateDate",
                value: new DateTime(2020, 10, 6, 19, 33, 0, 695, DateTimeKind.Utc).AddTicks(588));
        }
    }
}
