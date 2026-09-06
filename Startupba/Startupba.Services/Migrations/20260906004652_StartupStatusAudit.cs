using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class StartupStatusAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAt",
                table: "Startups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PausedByUserId",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedByUserId",
                table: "Startups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StartupStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartupStatusHistories_StartupStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "StartupStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StartupStatusHistories_StartupStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "StartupStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StartupStatusHistories_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartupStatusHistories_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Startups",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ApprovedByUserId", "PausedAt", "PausedByUserId", "RejectedByUserId" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Startups_ApprovedByUserId",
                table: "Startups",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_PausedByUserId",
                table: "Startups",
                column: "PausedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_RejectedByUserId",
                table: "Startups",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupStatusHistories_ActorUserId",
                table: "StartupStatusHistories",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupStatusHistories_FromStatusId",
                table: "StartupStatusHistories",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupStatusHistories_StartupId",
                table: "StartupStatusHistories",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupStatusHistories_ToStatusId",
                table: "StartupStatusHistories",
                column: "ToStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Startups_Users_ApprovedByUserId",
                table: "Startups",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Startups_Users_PausedByUserId",
                table: "Startups",
                column: "PausedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Startups_Users_RejectedByUserId",
                table: "Startups",
                column: "RejectedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Startups_Users_ApprovedByUserId",
                table: "Startups");

            migrationBuilder.DropForeignKey(
                name: "FK_Startups_Users_PausedByUserId",
                table: "Startups");

            migrationBuilder.DropForeignKey(
                name: "FK_Startups_Users_RejectedByUserId",
                table: "Startups");

            migrationBuilder.DropTable(
                name: "StartupStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Startups_ApprovedByUserId",
                table: "Startups");

            migrationBuilder.DropIndex(
                name: "IX_Startups_PausedByUserId",
                table: "Startups");

            migrationBuilder.DropIndex(
                name: "IX_Startups_RejectedByUserId",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "PausedByUserId",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "Startups");
        }
    }
}
