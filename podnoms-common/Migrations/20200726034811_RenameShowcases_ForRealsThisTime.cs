using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class RenameShowcases_ForRealsThisTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerShowcases",
                table: "ServerShowcases");

            migrationBuilder.RenameTable(
                name: "ServerShowcases",
                newName: "SiteMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteMessages",
                table: "SiteMessages",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteMessages",
                table: "SiteMessages");

            migrationBuilder.RenameTable(
                name: "SiteMessages",
                newName: "ServerShowcases");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerShowcases",
                table: "ServerShowcases",
                column: "Id");
        }
    }
}
