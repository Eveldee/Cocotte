using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cocotte.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityPlayerHasCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompleted",
                table: "ActivityPlayers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompleted",
                table: "ActivityPlayers");
        }
    }
}
