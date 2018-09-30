using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class AddedPrivatePodcastFields : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<byte[]>(
                name: "AuthPassword",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AuthPasswordSalt",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthUserName",
                table: "Podcasts",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Private",
                table: "Podcasts",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "AuthPassword",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "AuthUserName",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "Private",
                table: "Podcasts");
        }
    }
}