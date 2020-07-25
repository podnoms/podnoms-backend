using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class AddPlaylistIdToPodcastEntry : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_PodcastEntries_Playlists_PlaylistId",
                table: "PodcastEntries");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "PodcastEntries",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_PodcastEntries_Playlists_PlaylistId",
                table: "PodcastEntries",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_PodcastEntries_Playlists_PlaylistId",
                table: "PodcastEntries");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "PodcastEntries",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PodcastEntries_Playlists_PlaylistId",
                table: "PodcastEntries",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
