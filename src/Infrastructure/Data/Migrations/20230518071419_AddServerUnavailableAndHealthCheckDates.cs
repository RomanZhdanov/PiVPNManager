using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiVPNManager.Infrastructure.Data.Migrations
{
    public partial class AddServerUnavailableAndHealthCheckDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Dead",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHealthCheck",
                table: "Servers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnavailableSince",
                table: "Servers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dead",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "LastHealthCheck",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "UnavailableSince",
                table: "Servers");
        }
    }
}
