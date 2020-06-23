using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddInitialCommentsModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntryComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CommentText = table.Column<string>(nullable: true),
                    FromUser = table.Column<string>(nullable: true),
                    FromUserEmail = table.Column<string>(nullable: true),
                    Timestamp = table.Column<int>(nullable: true),
                    PodcastEntryId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryComments_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryComments_PodcastEntryId",
                table: "EntryComments",
                column: "PodcastEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryComments");
        }
    }
}
