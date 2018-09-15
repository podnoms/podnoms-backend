using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Api.Migrations
{
    public partial class AddPKtoPodcastCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Podcasts_PodcastId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_PodcastId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PodcastId",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "PodcastCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    PodcastId = table.Column<Guid>(nullable: false),
                    CategoryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PodcastCategory_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastCategory_CategoryId",
                table: "PodcastCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastCategory_PodcastId",
                table: "PodcastCategory",
                column: "PodcastId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastCategory");

            migrationBuilder.AddColumn<Guid>(
                name: "PodcastId",
                table: "Categories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_PodcastId",
                table: "Categories",
                column: "PodcastId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Podcasts_PodcastId",
                table: "Categories",
                column: "PodcastId",
                principalTable: "Podcasts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
