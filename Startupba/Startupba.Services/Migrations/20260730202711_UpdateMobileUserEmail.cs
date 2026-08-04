using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMobileUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Email",
                value: "startup.ba.support1@gmail.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Email",
                value: "mobile@startupba.com");
        }
    }
}
