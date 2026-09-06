using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class AnnouncementImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Announcements",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageData",
                value: null);

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageData",
                value: null);

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageData",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Announcements");
        }
    }
}
