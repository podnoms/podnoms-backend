using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class AddTagsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntryTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryTagPodcastEntry",
                columns: table => new
                {
                    EntriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTagPodcastEntry", x => new { x.EntriesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_EntryTagPodcastEntry_EntryTags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "EntryTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntryTagPodcastEntry_PodcastEntries_EntriesId",
                        column: x => x.EntriesId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryTagPodcastEntry_TagsId",
                table: "EntryTagPodcastEntry",
                column: "TagsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryTagPodcastEntry");

            migrationBuilder.DropTable(
                name: "EntryTags");
        }
    }
}
