using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddedSharingEntryListIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks");

            migrationBuilder.AlterColumn<string>(
                name: "LinkId",
                table: "PodcastEntrySharingLinks",
                nullable: true,
                oldClrType: typeof(string),
                oldDefaultValue: "False");

            migrationBuilder.AddColumn<int>(
                name: "LinkIndex",
                table: "PodcastEntrySharingLinks",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks",
                column: "LinkId",
                unique: true,
                filter: "[LinkId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkIndex",
                table: "PodcastEntrySharingLinks",
                column: "LinkIndex",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks");

            migrationBuilder.DropIndex(
                name: "IX_PodcastEntrySharingLinks_LinkIndex",
                table: "PodcastEntrySharingLinks");

            migrationBuilder.DropColumn(
                name: "LinkIndex",
                table: "PodcastEntrySharingLinks");

            migrationBuilder.AlterColumn<string>(
                name: "LinkId",
                table: "PodcastEntrySharingLinks",
                nullable: false,
                defaultValue: "False",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks",
                column: "LinkId");
        }
    }
}
