using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddPatreonTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "PatreonTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    AccessToken = table.Column<string>(nullable: true),
                    ExpiresIn = table.Column<long>(nullable: false),
                    TokenType = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    AppUserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatreonTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatreonTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatreonTokens_AppUserId",
                table: "PatreonTokens",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatreonTokens");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
