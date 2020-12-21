using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class CorrectUserIdTypeInServiceKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "ServiceApiKeys",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ServiceApiKeys",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceApiKeys_UserId",
                table: "ServiceApiKeys",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceApiKeys_AspNetUsers_UserId",
                table: "ServiceApiKeys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceApiKeys_AspNetUsers_UserId",
                table: "ServiceApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_ServiceApiKeys_UserId",
                table: "ServiceApiKeys");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ServiceApiKeys");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ServiceApiKeys");
        }
    }
}
