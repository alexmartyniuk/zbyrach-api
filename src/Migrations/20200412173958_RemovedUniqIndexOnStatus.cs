using Microsoft.EntityFrameworkCore.Migrations;

namespace Zbyrach.Api.Migrations
{
    public partial class RemovedUniqIndexOnStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticleUsers_Status",
                table: "ArticleUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ArticleUsers_Status",
                table: "ArticleUsers",
                column: "Status",
                unique: true);
        }
    }
}
