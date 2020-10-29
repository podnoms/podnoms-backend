using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class ChangeAudioLengthFromFloatToDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AudioLength",
                table: "PodcastEntries",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "AudioLength",
                table: "PodcastEntries",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
