using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddGeocodeStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "ActivityLogPodcastEntry",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zip",
                table: "ActivityLogPodcastEntry",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "Zip",
                table: "ActivityLogPodcastEntry");
        }
    }
}
