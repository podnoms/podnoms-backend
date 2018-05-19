using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Api.Migrations {
    public partial class ChangedPodcastIdType : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropIndex(
                name: "IX_Playlists_PodcastId",
                table: "Playlists");

            migrationBuilder.DropColumn("PodcastId", "Playlists");
            migrationBuilder.AddColumn<Guid>(
                name: "PodcastId",
                table: "Playlists",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_PodcastId",
                table: "Playlists",
                column: "PodcastId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Podcasts_PodcastId",
                table: "Playlists",
                column: "PodcastId",
                principalTable: "Podcasts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_Podcasts_PodcastId",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_PodcastId",
                table: "Playlists");

            migrationBuilder.AlterColumn<string>(
                name: "PodcastId",
                table: "Playlists",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "PodcastId1",
                table: "Playlists",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_PodcastId1",
                table: "Playlists",
                column: "PodcastId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_Podcasts_PodcastId1",
                table: "Playlists",
                column: "PodcastId1",
                principalTable: "Podcasts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
