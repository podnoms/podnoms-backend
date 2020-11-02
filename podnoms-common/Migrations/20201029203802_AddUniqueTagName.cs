using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class AddUniqueTagName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "EntryTags",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryTags_TagName",
                table: "EntryTags",
                column: "TagName",
                unique: true,
                filter: "[TagName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryTags_TagName",
                table: "EntryTags");

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "EntryTags",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
