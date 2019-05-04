using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class NotificationsConfig : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "NotificationUrl",
                table: "Notifications",
                newName: "Config");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "Config",
                table: "Notifications",
                newName: "NotificationUrl");
        }
    }
}