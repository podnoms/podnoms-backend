using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations {
    public partial class RenameServiceToApiKeys : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameTable("ServicesApiKeys", newName: "ServiceApiKeys");
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameTable("ServiceApiKeys", newName: "ServicesApiKeys");
        }
    }
}
