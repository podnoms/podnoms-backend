using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Comon.Migrations
{
    public partial class AddPodcastAggregators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastAggregators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    PodcastId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastAggregators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastAggregators_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastAggregators_PodcastId",
                table: "PodcastAggregators",
                column: "PodcastId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastAggregators");
        }
    }
}
