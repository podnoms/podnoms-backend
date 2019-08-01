using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class NukeParsedPlaylists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsedPlaylistItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceCreateDate",
                table: "PodcastEntries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCreateDate",
                table: "PodcastEntries");

            migrationBuilder.CreateTable(
                name: "ParsedPlaylistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    IsProcessed = table.Column<bool>(nullable: false),
                    PlaylistId = table.Column<Guid>(nullable: false),
                    PlaylistItemDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    VideoId = table.Column<string>(nullable: true),
                    VideoType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedPlaylistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedPlaylistItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPlaylistItems_PlaylistId",
                table: "ParsedPlaylistItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPlaylistItems_VideoId_PlaylistId",
                table: "ParsedPlaylistItems",
                columns: new[] { "VideoId", "PlaylistId" },
                unique: true,
                filter: "[VideoId] IS NOT NULL");
        }
    }
}
