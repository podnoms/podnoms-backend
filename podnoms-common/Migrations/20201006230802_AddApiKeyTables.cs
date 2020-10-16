using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class AddApiKeyTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicesApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicesApiKeyLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stack = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
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

            migrationBuilder.CreateIndex(
                name: "IX_ServicesApiKeyLogs_ApiKeyId",
                table: "ServicesApiKeyLogs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesApiKeyLogs_UserId",
                table: "ServicesApiKeyLogs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicesApiKeyLogs");

            migrationBuilder.DropTable(
                name: "ServicesApiKeys");
        }
    }
}
