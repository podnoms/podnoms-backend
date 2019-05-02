using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddedSubscriptions_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSubscription_AspNetUsers_AppUserId",
                table: "AccountSubscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountSubscription",
                table: "AccountSubscription");

            migrationBuilder.RenameTable(
                name: "AccountSubscription",
                newName: "AccountSubscriptions");

            migrationBuilder.RenameIndex(
                name: "IX_AccountSubscription_AppUserId",
                table: "AccountSubscriptions",
                newName: "IX_AccountSubscriptions_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountSubscriptions",
                table: "AccountSubscriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSubscriptions_AspNetUsers_AppUserId",
                table: "AccountSubscriptions",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSubscriptions_AspNetUsers_AppUserId",
                table: "AccountSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountSubscriptions",
                table: "AccountSubscriptions");

            migrationBuilder.RenameTable(
                name: "AccountSubscriptions",
                newName: "AccountSubscription");

            migrationBuilder.RenameIndex(
                name: "IX_AccountSubscriptions_AppUserId",
                table: "AccountSubscription",
                newName: "IX_AccountSubscription_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountSubscription",
                table: "AccountSubscription",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSubscription_AspNetUsers_AppUserId",
                table: "AccountSubscription",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
