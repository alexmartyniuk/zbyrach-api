using Microsoft.EntityFrameworkCore.Migrations;

namespace Zbyrach.Api.Migrations
{
    public partial class FavoriteAndReadLater : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Favorite",
                table: "ArticleUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReadLater",
                table: "ArticleUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Favorite",
                table: "ArticleUsers");

            migrationBuilder.DropColumn(
                name: "ReadLater",
                table: "ArticleUsers");
        }
    }
}
