using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class StatusToNotificationLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Succeeded",
                table: "NotificationLogs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ActivityLogPodcastEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    PodcastEntryId = table.Column<Guid>(nullable: true),
                    ClientAddress = table.Column<string>(nullable: true),
                    ExtraInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogPodcastEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogPodcastEntry_PodcastEntryId",
                table: "ActivityLogPodcastEntry",
                column: "PodcastEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogPodcastEntry");

            migrationBuilder.DropColumn(
                name: "Succeeded",
                table: "NotificationLogs");
        }
    }
}
