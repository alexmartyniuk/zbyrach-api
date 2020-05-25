using Microsoft.EntityFrameworkCore.Migrations;

namespace Zbyrach.Api.Migrations
{
    public partial class RemoveArticleFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Articles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Articles",
                type: "TEXT",
                nullable: true);
        }
    }
}
