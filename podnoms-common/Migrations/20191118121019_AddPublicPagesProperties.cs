using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddPublicPagesProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicTitle",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "Podcasts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "PublicTitle",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Podcasts");
        }
    }
}
