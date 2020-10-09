using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Zbyrach.Api.Migrations
{
    public partial class TokenCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "AccessTokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AccessTokens",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_AccessTokens_ClientIp",
                table: "AccessTokens",
                column: "ClientIp");

            migrationBuilder.CreateIndex(
                name: "IX_AccessTokens_ClientUserAgent",
                table: "AccessTokens",
                column: "ClientUserAgent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessTokens_ClientIp",
                table: "AccessTokens");

            migrationBuilder.DropIndex(
                name: "IX_AccessTokens_ClientUserAgent",
                table: "AccessTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AccessTokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "AccessTokens",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
