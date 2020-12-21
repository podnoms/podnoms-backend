using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations {
    public partial class AddEnabledToApiKeys : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "ServicesApiKeys",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "ServicesApiKeys");
        }
    }
}
