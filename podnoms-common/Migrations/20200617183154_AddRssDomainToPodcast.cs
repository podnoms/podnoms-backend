using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddRssDomainToPodcast : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomRssDomain",
                table: "Podcasts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomRssDomain",
                table: "Podcasts");
        }
    }
}
