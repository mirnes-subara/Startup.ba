using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostSharedFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharedFromBlogPostId",
                table: "BlogPosts",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 1,
                column: "SharedFromBlogPostId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 2,
                column: "SharedFromBlogPostId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 3,
                column: "SharedFromBlogPostId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 4,
                column: "SharedFromBlogPostId",
                value: null);

            migrationBuilder.UpdateData(
                table: "BlogPosts",
                keyColumn: "Id",
                keyValue: 5,
                column: "SharedFromBlogPostId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_SharedFromBlogPostId",
                table: "BlogPosts",
                column: "SharedFromBlogPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_BlogPosts_SharedFromBlogPostId",
                table: "BlogPosts",
                column: "SharedFromBlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_BlogPosts_SharedFromBlogPostId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_SharedFromBlogPostId",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "SharedFromBlogPostId",
                table: "BlogPosts");
        }
    }
}
