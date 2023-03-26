using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cocotte.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizedActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizer",
                table: "ActivityPlayers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOrganizer",
                table: "ActivityPlayers");
        }
    }
}
