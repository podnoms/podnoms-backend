using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class AddServicesApiKeyUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicesApiKeyLogs_AspNetUsers_UserId",
                table: "ServicesApiKeyLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicesApiKeyLogs_ServicesApiKeys_ApiKeyId",
                table: "ServicesApiKeyLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicesApiKeyLogs",
                table: "ServicesApiKeyLogs");

            migrationBuilder.RenameTable(
                name: "ServicesApiKeyLogs",
                newName: "ServicesApiKeyLog");

            migrationBuilder.RenameIndex(
                name: "IX_ServicesApiKeyLogs_UserId",
                table: "ServicesApiKeyLog",
                newName: "IX_ServicesApiKeyLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicesApiKeyLogs_ApiKeyId",
                table: "ServicesApiKeyLog",
                newName: "IX_ServicesApiKeyLog_ApiKeyId");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "ServicesApiKeys",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicesApiKeyLog",
                table: "ServicesApiKeyLog",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesApiKeyLog_AspNetUsers_UserId",
                table: "ServicesApiKeyLog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesApiKeyLog_ServicesApiKeys_ApiKeyId",
                table: "ServicesApiKeyLog",
                column: "ApiKeyId",
                principalTable: "ServicesApiKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicesApiKeyLog_AspNetUsers_UserId",
                table: "ServicesApiKeyLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicesApiKeyLog_ServicesApiKeys_ApiKeyId",
                table: "ServicesApiKeyLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicesApiKeyLog",
                table: "ServicesApiKeyLog");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "ServicesApiKeys");

            migrationBuilder.RenameTable(
                name: "ServicesApiKeyLog",
                newName: "ServicesApiKeyLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ServicesApiKeyLog_UserId",
                table: "ServicesApiKeyLogs",
                newName: "IX_ServicesApiKeyLogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicesApiKeyLog_ApiKeyId",
                table: "ServicesApiKeyLogs",
                newName: "IX_ServicesApiKeyLogs_ApiKeyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicesApiKeyLogs",
                table: "ServicesApiKeyLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesApiKeyLogs_AspNetUsers_UserId",
                table: "ServicesApiKeyLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesApiKeyLogs_ServicesApiKeys_ApiKeyId",
                table: "ServicesApiKeyLogs",
                column: "ApiKeyId",
                principalTable: "ServicesApiKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
