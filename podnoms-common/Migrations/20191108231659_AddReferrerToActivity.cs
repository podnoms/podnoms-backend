using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddReferrerToActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncomingUrl",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.AddColumn<string>(
                name: "Referrer",
                table: "ActivityLogPodcastEntry",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referrer",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.AddColumn<string>(
                name: "IncomingUrl",
                table: "ActivityLogPodcastEntry",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
