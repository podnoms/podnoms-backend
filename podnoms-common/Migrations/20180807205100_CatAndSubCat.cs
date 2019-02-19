using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class CatAndSubCat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Podcasts_Subcategories_SubcategoryId",
                table: "Podcasts");

            migrationBuilder.DropIndex(
                name: "IX_Podcasts_SubcategoryId",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "SubcategoryId",
                table: "Podcasts");

            migrationBuilder.AddColumn<Guid>(
                name: "PodcastId",
                table: "Subcategories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_PodcastId",
                table: "Subcategories",
                column: "PodcastId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subcategories_Podcasts_PodcastId",
                table: "Subcategories",
                column: "PodcastId",
                principalTable: "Podcasts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subcategories_Podcasts_PodcastId",
                table: "Subcategories");

            migrationBuilder.DropIndex(
                name: "IX_Subcategories_PodcastId",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "PodcastId",
                table: "Subcategories");

            migrationBuilder.AddColumn<Guid>(
                name: "SubcategoryId",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_SubcategoryId",
                table: "Podcasts",
                column: "SubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Podcasts_Subcategories_SubcategoryId",
                table: "Podcasts",
                column: "SubcategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
