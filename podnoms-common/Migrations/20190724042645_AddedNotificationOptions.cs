using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class AddedNotificationOptions : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                name: "EmailNotificationOptions",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 31);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "EmailNotificationOptions",
                table: "AspNetUsers");
        }
    }
}
