using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddUserIdToSlugRedirectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserSlugRedirects_AspNetUsers_ApplicationUserId",
                table: "ApplicationUserSlugRedirects");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserSlugRedirects_ApplicationUserId",
                table: "ApplicationUserSlugRedirects");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicationUserId",
                table: "ApplicationUserSlugRedirects",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "ApplicationUserSlugRedirects",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserSlugRedirects_ApplicationUserId1",
                table: "ApplicationUserSlugRedirects",
                column: "ApplicationUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserSlugRedirects_AspNetUsers_ApplicationUserId1",
                table: "ApplicationUserSlugRedirects",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserSlugRedirects_AspNetUsers_ApplicationUserId1",
                table: "ApplicationUserSlugRedirects");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserSlugRedirects_ApplicationUserId1",
                table: "ApplicationUserSlugRedirects");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "ApplicationUserSlugRedirects");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ApplicationUserSlugRedirects",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserSlugRedirects_ApplicationUserId",
                table: "ApplicationUserSlugRedirects",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserSlugRedirects_AspNetUsers_ApplicationUserId",
                table: "ApplicationUserSlugRedirects",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
