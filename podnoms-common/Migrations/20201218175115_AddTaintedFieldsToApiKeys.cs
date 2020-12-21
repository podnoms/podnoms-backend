using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class AddTaintedFieldsToApiKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Tainted",
                table: "ServicesApiKeys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaintedDate",
                table: "ServicesApiKeys",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaintedReason",
                table: "ServicesApiKeys",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tainted",
                table: "ServicesApiKeys");

            migrationBuilder.DropColumn(
                name: "TaintedDate",
                table: "ServicesApiKeys");

            migrationBuilder.DropColumn(
                name: "TaintedReason",
                table: "ServicesApiKeys");
        }
    }
}
