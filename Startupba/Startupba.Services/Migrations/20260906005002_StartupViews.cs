using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class StartupViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StartupViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartupViews_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartupViews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StartupViews_StartupId_UserId",
                table: "StartupViews",
                columns: new[] { "StartupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StartupViews_UserId",
                table: "StartupViews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StartupViews");
        }
    }
}
