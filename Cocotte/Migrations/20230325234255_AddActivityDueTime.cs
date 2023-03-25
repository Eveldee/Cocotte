using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cocotte.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityDueTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "DueTime",
                table: "Activities",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "Activities");
        }
    }
}
