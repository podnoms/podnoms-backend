using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class ActivityLogCascadeDeletes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                table: "ActivityLogPodcastEntry",
                column: "PodcastEntryId",
                principalTable: "PodcastEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                table: "ActivityLogPodcastEntry");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                table: "ActivityLogPodcastEntry",
                column: "PodcastEntryId",
                principalTable: "PodcastEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
