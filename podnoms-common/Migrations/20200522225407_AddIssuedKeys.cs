using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddIssuedKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssuedApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    Name = table.Column<string>(nullable: true),
                    Prefix = table.Column<string>(maxLength: 7, nullable: true),
                    Key = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    Expires = table.Column<DateTime>(nullable: true),
                    IssuedToId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuedApiKeys_AspNetUsers_IssuedToId",
                        column: x => x.IssuedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssuedApiKeys_IssuedToId",
                table: "IssuedApiKeys",
                column: "IssuedToId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssuedApiKeys");
        }
    }
}
