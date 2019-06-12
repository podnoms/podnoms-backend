using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations {
    public partial class FixCascadeOnSharingLink : Migration {
        protected override void Up (MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey (
                name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                table: "PodcastEntrySharingLinks");
            migrationBuilder.AddForeignKey (
                name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                table: "PodcastEntrySharingLinks",
                column: "PodcastEntryId",
                principalTable: "PodcastEntries",
                principalColumn: "Id",
                onDelete : ReferentialAction.Cascade);
        }

        protected override void Down (MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey (
                name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                table: "PodcastEntrySharingLinks");

            migrationBuilder.AddForeignKey (
                name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                table: "PodcastEntrySharingLinks",
                column: "PodcastEntryId",
                principalTable: "PodcastEntries",
                principalColumn: "Id",
                onDelete : ReferentialAction.Restrict);
        }
    }
}
