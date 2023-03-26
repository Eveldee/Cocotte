using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cocotte.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ThreadId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatorUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatorDisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    DueDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<int>(type: "INTEGER", nullable: false),
                    AreRolesEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxPlayers = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<int>(type: "INTEGER", nullable: true),
                    Stage = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => new { x.GuildId, x.ChannelId, x.MessageId });
                });

            migrationBuilder.CreateTable(
                name: "ActivityPlayers",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    Roles = table.Column<byte>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityPlayers", x => new { x.GuildId, x.ChannelId, x.MessageId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ActivityPlayers_Activities_GuildId_ChannelId_MessageId",
                        columns: x => new { x.GuildId, x.ChannelId, x.MessageId },
                        principalTable: "Activities",
                        principalColumns: new[] { "GuildId", "ChannelId", "MessageId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ThreadId",
                table: "Activities",
                column: "ThreadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityPlayers");

            migrationBuilder.DropTable(
                name: "Activities");
        }
    }
}
