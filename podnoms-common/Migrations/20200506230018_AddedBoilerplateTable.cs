using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddedBoilerplateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoilerPlates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    Key = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerPlates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoilerPlates");
        }
    }
}
