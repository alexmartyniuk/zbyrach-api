using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MediumGrabber.Api.Migrations
{
    public partial class DatesInMailingSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSentAt",
                table: "MailingSettings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MailingSettings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSentAt",
                table: "MailingSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MailingSettings");
        }
    }
}
