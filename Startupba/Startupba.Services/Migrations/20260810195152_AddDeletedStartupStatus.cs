using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedStartupStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StartupStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[] { 7, "Startup was removed by the founder", true, "Deleted" });

            migrationBuilder.Sql("UPDATE Startups SET StatusId = 7 WHERE IsActive = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Startups SET StatusId = 4 WHERE StatusId = 7");

            migrationBuilder.DeleteData(
                table: "StartupStatuses",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
