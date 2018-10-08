using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddNotificationLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    NotificationId = table.Column<Guid>(nullable: false),
                    Log = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_NotificationId",
                table: "NotificationLogs",
                column: "NotificationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLogs");
        }
    }
}
