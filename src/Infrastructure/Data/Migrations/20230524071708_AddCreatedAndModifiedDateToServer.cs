using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiVPNManager.Infrastructure.Data.Migrations
{
    public partial class AddCreatedAndModifiedDateToServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Servers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Servers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Servers");
        }
    }
}
