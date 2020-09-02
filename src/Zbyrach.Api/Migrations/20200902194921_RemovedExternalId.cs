using Microsoft.EntityFrameworkCore.Migrations;

namespace Zbyrach.Api.Migrations
{
    public partial class RemovedExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Articles_ExternalId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Articles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ExternalId",
                table: "Articles",
                column: "ExternalId",
                unique: true);
        }
    }
}
