using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Migrations {
    [DbContext(typeof(PodNomsDbContext))]
    [Migration("20180519081555_UndoTheShite")]


    public partial class RemovedIdField : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {

            migrationBuilder.AddColumn<string>("IdBuffer", "Podcasts", "int", nullable: true);
            migrationBuilder.AddColumn<string>("IdBuffer", "PodcastEntries", "int", nullable: true);

            migrationBuilder.Sql("UPDATE Podcasts SET IdBuffer = Id");
            migrationBuilder.Sql("UPDATE PodcastEntries SET IdBuffer = PodcastId");
            migrationBuilder.Sql("UPDATE Podcasts SET Uid = newid() wHERE Uid IS NULL");
            migrationBuilder.Sql("UPDATE PodcastEntries SET Uid = newid() wHERE Uid IS NULL");

            migrationBuilder.DropForeignKey(name: "FK_PodcastEntries_Podcasts_PodcastId",
                table: "PodcastEntries");
            migrationBuilder.DropForeignKey(name: "FK_Playlists_Podcasts_PodcastId",
                table: "Playlists");
            migrationBuilder.DropPrimaryKey("PK_PodcastEntries", "PodcastEntries");
            migrationBuilder.DropPrimaryKey("PK_Podcasts", "Podcasts");

            migrationBuilder.DropIndex(
                name: "IX_PodcastEntries_PodcastId",
                table: "PodcastEntries");

            migrationBuilder.AlterColumn<string>(
                                name: "PodcastId",
                            table: "PodcastEntries",
                            nullable: true,
                            oldClrType: typeof(string),
                            oldNullable: true);

            migrationBuilder.DropColumn("Id", "Podcasts");
            migrationBuilder.DropColumn("Id", "PodcastEntries");
            migrationBuilder.RenameColumn("Uid", "Podcasts", "Id");
            migrationBuilder.RenameColumn("Uid", "PodcastEntries", "Id");

            migrationBuilder.Sql("UPDATE PodcastEntries SET PodcastId = Podcasts.Id FROM Podcasts INNER JOIN PodcastEntries ON Podcasts.IdBuffer = PodcastEntries.IdBuffer");
            migrationBuilder.AlterColumn<Guid>(
                                name: "Id",
                            table: "Podcasts",
                            nullable: false,
                            oldClrType: typeof(string),
                            oldNullable: true);
            migrationBuilder.AlterColumn<Guid>(
                                name: "Id",
                            table: "PodcastEntries",
                            nullable: false,
                            oldClrType: typeof(string),
                            oldNullable: true);
            migrationBuilder.AlterColumn<Guid>(
                                name: "PodcastId",
                            table: "PodcastEntries",
                            nullable: false,
                            oldClrType: typeof(string),
                            oldNullable: true);

            migrationBuilder.AddPrimaryKey("PK_Podcasts", "Podcasts", "Id");
            migrationBuilder.AddPrimaryKey("PK_PodcastEntries", "PodcastEntries", "Id");
            migrationBuilder.AddForeignKey(
                "FK_PodcastEntries_Podcasts_PodcastId",
                "PodcastEntries",
                column: "PodcastId",
                principalTable: "Podcasts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntries_PodcastId",
                table: "PodcastEntries",
                column: "PodcastId");

        }

        protected override void Down(MigrationBuilder migrationBuilder) {

        }
    }
}
