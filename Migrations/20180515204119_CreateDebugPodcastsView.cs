using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Api.Migrations {
    public partial class CreateDebugPodcastsView : Migration {
        const string SQL =
         @"IF OBJECT_ID('debug.FullPodcast') IS NULL
            BEGIN
                EXEC('CREATE VIEW debug.FullPodcast
                AS
                    SELECT
                        Podcasts.Id AS PodcastId,
                        Podcasts.Title AS PodcastTitle,
                        Podcasts.Slug AS PodcastSlug,
                        Podcasts.CreateDate AS PodcastCreateDate,
                        AspNetUsers.FirstName + '' '' + AspNetUsers.Lastname AS UserName,
                        AspNetUsers.Email AS UserEmail,
                        PodcastEntries.Id AS EntryId,
                        PodcastEntries.NewId AS EntryUId,
                        PodcastEntries.Title AS EntryTitle,
                        PodcastEntries.SourceUrl,
                        PodcastEntries.ProcessingPayload,
                        PodcastEntries.Processed
                    FROM Podcasts
                        LEFT OUTER JOIN PodcastEntries ON PodcastEntries.PodcastId = Podcasts.Id
                        LEFT OUTER JOIN AspNetUsers ON Podcasts.AppUserId = AspNetUsers.Id')
            END";
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.EnsureSchema("debug");
            migrationBuilder.Sql(SQL);
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("DROP VIEW debug.FullPodcast");
            migrationBuilder.DropSchema("debug");
        }
    }
}
