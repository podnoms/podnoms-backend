using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddedSharingEntryList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastEntrySharingLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    LinkId = table.Column<string>(nullable: false, defaultValue: "False"),
                    PodcastEntryId = table.Column<Guid>(nullable: true),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEntrySharingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks",
                column: "LinkId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_PodcastEntryId",
                table: "PodcastEntrySharingLinks",
                column: "PodcastEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastEntrySharingLinks");
        }
    }
}
