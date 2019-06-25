using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class UniqueSourceUrlInPlaylist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SourceUrl",
                table: "Playlists",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_SourceUrl",
                table: "Playlists",
                column: "SourceUrl",
                unique: true,
                filter: "[SourceUrl] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Playlists_SourceUrl",
                table: "Playlists");

            migrationBuilder.AlterColumn<string>(
                name: "SourceUrl",
                table: "Playlists",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
