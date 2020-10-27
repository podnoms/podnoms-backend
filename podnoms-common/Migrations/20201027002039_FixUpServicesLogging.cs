using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations {
    public partial class FixUpServicesLogging : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicesApiKeyLog_AspNetUsers_UserId",
                table: "ServicesApiKeyLog");

            migrationBuilder.DropIndex(
                name: "IX_ServicesApiKeyLog_UserId",
                table: "ServicesApiKeyLog");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ServicesApiKeyLog");

            migrationBuilder.AddColumn<string>(
                name: "RequesterId",
                table: "ServicesApiKeyLog",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "ServicesApiKeyLog");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ServicesApiKeyLog",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicesApiKeyLog_UserId",
                table: "ServicesApiKeyLog",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesApiKeyLog_AspNetUsers_UserId",
                table: "ServicesApiKeyLog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
