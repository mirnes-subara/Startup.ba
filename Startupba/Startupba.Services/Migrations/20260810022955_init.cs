using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Startupba.Services.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StartupStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Picture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsVerificationRequested = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GenderId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chats_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Startups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FounderId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountRaised = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformFeePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Startups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Startups_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Startups_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Startups_StartupStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "StartupStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Startups_Users_FounderId",
                        column: x => x.FounderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    StartupId = table.Column<int>(type: "int", nullable: true),
                    SharedFromBlogPostId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_BlogPosts_SharedFromBlogPostId",
                        column: x => x.SharedFromBlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlogPosts_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlogPosts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StartupImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    IsCover = table.Column<bool>(type: "bit", nullable: false),
                    IsLogo = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartupImages_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartupLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartupLikes_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartupLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlogPostLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogPostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPostLikes_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPostLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogPostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterId = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    StartupId = table.Column<int>(type: "int", nullable: true),
                    BlogPostId = table.Column<int>(type: "int", nullable: true),
                    ReportedUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reports_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationId = table.Column<int>(type: "int", nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StripeRefundId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BillingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Green technology, recycling and sustainability projects", true, "Ecology" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Financial technology, payments and banking innovation", true, "FinTech" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Digital health, medical devices and wellness solutions", true, "HealthTech" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Education technology and online learning platforms", true, "EdTech" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Agriculture technology and smart farming", true, "AgroTech" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Video games, esports and interactive entertainment", true, "Gaming" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Travel, hospitality and local experiences", true, "Tourism" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Food production, delivery and culinary innovation", true, "Food & Beverage" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "AI, machine learning and data-driven products", true, "Artificial Intelligence" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Online retail, marketplaces and logistics", true, "E-commerce" },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Transportation, ride sharing and urban mobility", true, "Mobility" },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Non-profit initiatives and community-driven projects", true, "Social Impact" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "BA", true, "Bosnia and Herzegovina" },
                    { 2, "HR", true, "Croatia" },
                    { 3, "RS", true, "Serbia" },
                    { 4, "DE", true, "Germany" },
                    { 5, "AT", true, "Austria" },
                    { 6, "SI", true, "Slovenia" },
                    { 7, "ME", true, "Montenegro" },
                    { 8, "MK", true, "North Macedonia" },
                    { 9, "IT", true, "Italy" },
                    { 10, "CH", true, "Switzerland" },
                    { 11, "NL", true, "Netherlands" },
                    { 12, "FR", true, "France" },
                    { 13, "GB", true, "United Kingdom" },
                    { 14, "US", true, "United States" },
                    { 15, "TR", true, "Turkey" }
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Male" },
                    { 2, "Female" }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "BillingAddress", "BillingCity", "BillingCountry", "BillingState", "BillingZipCode", "CreatedAt", "Currency", "CustomerEmail", "CustomerName", "DonationId", "PaymentMethod", "Status", "StripeCustomerId", "StripePaymentIntentId", "StripeRefundId", "UpdatedAt" },
                values: new object[] { 4, 500m, "Ferhadija 12", "Sarajevo", "Bosnia and Herzegovina", null, "71000", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "EUR", "investor1@startupba.com", "Sarah Miller", null, "card", "pending", "cus_seed_0000000004", "pi_seed_0000000004", null, null });

            migrationBuilder.InsertData(
                table: "PlatformSettings",
                columns: new[] { "Id", "Description", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "Percentage the platform keeps when a startup reaches its funding target", "PlatformFeePercent", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "5" },
                    { 2, "Terms of use displayed to users", "TermsOfUse", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Welcome to Startup.ba. By using this platform you agree to the following terms: 1) All startup submissions are reviewed by the administrator before publication. 2) The platform keeps a fee (see PlatformFeePercent) from the collected amount once a startup reaches its funding target. 3) Donations are voluntary contributions and do not represent equity or ownership. 4) Content that violates community standards may be removed and repeated violations may lead to account suspension. 5) Users are responsible for the accuracy of the information they publish." },
                    { 3, "Contact email displayed to users", "ContactEmail", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "support@startupba.com" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Full system access and administrative privileges", true, "Administrator" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Standard user - can create startups, invest, write posts and chat", true, "User" }
                });

            migrationBuilder.InsertData(
                table: "StartupStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Startup is being prepared and is not yet submitted", true, "Draft" },
                    { 2, "Startup is awaiting administrator review", true, "Pending" },
                    { 3, "Startup is approved and visible to investors", true, "Approved" },
                    { 4, "Startup was rejected by the administrator", true, "Rejected" },
                    { 5, "Startup is temporarily paused by the administrator", true, "Paused" },
                    { 6, "Startup reached its funding target", true, "Completed" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, "Sarajevo" },
                    { 2, 1, true, "Mostar" },
                    { 3, 1, true, "Banja Luka" },
                    { 4, 1, true, "Tuzla" },
                    { 5, 1, true, "Zenica" },
                    { 6, 2, true, "Zagreb" },
                    { 7, 2, true, "Split" },
                    { 8, 2, true, "Rijeka" },
                    { 9, 3, true, "Belgrade" },
                    { 10, 3, true, "Novi Sad" },
                    { 11, 4, true, "Berlin" },
                    { 12, 4, true, "Munich" },
                    { 13, 4, true, "Frankfurt" },
                    { 14, 5, true, "Vienna" },
                    { 15, 5, true, "Graz" },
                    { 16, 1, true, "Bihac" },
                    { 17, 1, true, "Brcko" },
                    { 18, 1, true, "Trebinje" },
                    { 19, 2, true, "Osijek" },
                    { 20, 2, true, "Zadar" },
                    { 21, 2, true, "Dubrovnik" },
                    { 22, 3, true, "Nis" },
                    { 23, 3, true, "Kragujevac" },
                    { 24, 4, true, "Hamburg" },
                    { 25, 4, true, "Cologne" },
                    { 26, 5, true, "Linz" },
                    { 27, 5, true, "Salzburg" },
                    { 28, 6, true, "Ljubljana" },
                    { 29, 6, true, "Maribor" },
                    { 30, 6, true, "Koper" },
                    { 31, 7, true, "Podgorica" },
                    { 32, 7, true, "Budva" },
                    { 33, 7, true, "Niksic" },
                    { 34, 8, true, "Skopje" },
                    { 35, 8, true, "Bitola" },
                    { 36, 8, true, "Ohrid" },
                    { 37, 9, true, "Rome" },
                    { 38, 9, true, "Milan" },
                    { 39, 9, true, "Naples" },
                    { 40, 9, true, "Turin" },
                    { 41, 10, true, "Zurich" },
                    { 42, 10, true, "Geneva" },
                    { 43, 10, true, "Bern" },
                    { 44, 11, true, "Amsterdam" },
                    { 45, 11, true, "Rotterdam" },
                    { 46, 11, true, "The Hague" },
                    { 47, 12, true, "Paris" },
                    { 48, 12, true, "Lyon" },
                    { 49, 12, true, "Marseille" },
                    { 50, 13, true, "London" },
                    { 51, 13, true, "Manchester" },
                    { 52, 13, true, "Edinburgh" },
                    { 53, 14, true, "New York" },
                    { 54, 14, true, "San Francisco" },
                    { 55, 14, true, "Chicago" },
                    { 56, 14, true, "Austin" },
                    { 57, 15, true, "Istanbul" },
                    { 58, 15, true, "Ankara" },
                    { 59, 15, true, "Izmir" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CityId", "CreatedAt", "Email", "FirstName", "GenderId", "IsActive", "IsVerificationRequested", "IsVerified", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "Picture", "Username" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@startupba.com", "James", 1, true, false, true, null, "Anderson", "1PPqaF2JFHRdZ6aqa3VMimfEBqeyv8AGuccho8s4MHk=", "aGk9AqtPuyMxuMw5kVMi5A==", "+387 61 111 111", null, "desktop" },
                    { 2, 1, new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "startup.ba.support1@gmail.com", "Adam", 1, true, false, true, null, "Foster", "17Zt8712G7zeS0f+zOGoHxmvZuXSTQ0hVaI8Zc353JQ=", "1STBoIEdpJWS+EEIXMAGPg==", "+387 61 111 111", null, "mobile" },
                    { 3, 2, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "founder2@startupba.com", "Emma", 2, true, false, true, null, "Clark", "nd+29z0ehe4DWNr8rZSlOqx0lffQKJPh2kq8QCmdw6U=", "eElvpyM+TaKIvPjtwWpHHw==", "+387 61 111 111", null, "founder2" },
                    { 4, 6, new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "founder3@startupba.com", "David", 1, true, false, true, null, "Novak", "IcWlJxfnmIwu+m0tzGLrgYDv0MK02WiG4tbCeujs6HU=", "Z7meJcRp90X4bS/7Y9NncA==", "+387 61 111 111", null, "founder3" },
                    { 5, 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "investor1@startupba.com", "Sarah", 2, true, false, true, null, "Miller", "NsyveZe4YeIzQTHOpH0KtyMnvAulRhhZjEAaEZGwR64=", "EbcFiMx6vIqIJsNFsY9DTQ==", "+387 61 111 111", null, "investor1" },
                    { 6, 9, new DateTime(2025, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "investor2@startupba.com", "Mark", 1, true, false, false, null, "Johnson", "fIbPExVnVzU9iqU9O5VR6V+Vr1DxO9z5U/SgRkiJIrc=", "of8eCvG1jGgNTYTz/C1n7g==", "+387 61 111 111", null, "investor2" },
                    { 7, 14, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "investor3@startupba.com", "Lena", 2, true, false, false, null, "Weber", "LiEnPXO5rRO5E5MTi2U7gYaP1gQwLNj+5v9ja2Byqh0=", "ob4080Eooa8D2avTDmXFVA==", "+387 61 111 111", null, "investor3" },
                    { 8, 11, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "founder4@startupba.com", "Tom", 1, true, false, false, null, "Becker", "aNhaOOAgSBCEJpZoe7ooikUH8HMLl4f9EQXYBLV5WJU=", "6j4bFSWcwXYY2y6OiBRTXg==", "+387 61 111 111", null, "founder4" }
                });

            migrationBuilder.InsertData(
                table: "Announcements",
                columns: new[] { "Id", "Content", "CreatedAt", "CreatedByUserId", "IsActive", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "We are excited to launch Startup.ba - the crowdfunding platform for the regional startup community. Browse startups, support the ideas you believe in, share your own project and connect with other founders and investors. Every new startup is reviewed by our team before going live to keep the platform safe and trustworthy.", new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, "Welcome to Startup.ba!", null },
                    { 2, "Based on your feedback we added three new categories: Artificial Intelligence, Mobility and Social Impact. If your startup fits one of them better, you can update the category from your startup's edit page.", new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, "New Categories Added", null },
                    { 3, "Startup.ba will be unavailable on February 15 from 02:00 to 04:00 CET due to planned infrastructure maintenance. No action is required - all data and running campaigns remain unaffected.", new DateTime(2025, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, "Scheduled Maintenance on February 15", null }
                });

            migrationBuilder.InsertData(
                table: "BlogPosts",
                columns: new[] { "Id", "AuthorId", "Content", "CreatedAt", "ImageData", "IsActive", "SharedFromBlogPostId", "StartupId", "Title", "UpdatedAt" },
                values: new object[] { 4, 5, "After supporting a dozen projects on this platform, here are the five things I always check before donating: a clear problem statement, a realistic funding target, a concrete plan for the money, an active founder who answers questions, and community engagement on the startup page.", new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, null, "What I Look for Before Investing in a Startup", null });

            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "IsRead", "Message", "ReadAt", "ReceiverId", "SenderId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), true, "Hi Adam! I saw GreenCycle and I love the concept. How durable are the smart bins?", new DateTime(2025, 9, 9, 10, 15, 0, 0, DateTimeKind.Unspecified), 2, 5 },
                    { 2, new DateTime(2025, 9, 9, 10, 20, 0, 0, DateTimeKind.Unspecified), true, "Hi Sarah, thanks! The casing is recycled aluminum, rated for 10 years outdoors.", new DateTime(2025, 9, 9, 10, 25, 0, 0, DateTimeKind.Unspecified), 5, 2 },
                    { 3, new DateTime(2025, 9, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), true, "Impressive. Just sent a donation - good luck with the pilot!", new DateTime(2025, 9, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), 2, 5 },
                    { 4, new DateTime(2025, 9, 10, 9, 45, 0, 0, DateTimeKind.Unspecified), true, "Thank you so much! I'll keep you posted on the progress.", new DateTime(2025, 9, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), 5, 2 },
                    { 5, new DateTime(2025, 9, 26, 14, 0, 0, 0, DateTimeKind.Unspecified), true, "Hello Emma, does PayLink plan to support business accounts?", new DateTime(2025, 9, 26, 15, 0, 0, 0, DateTimeKind.Unspecified), 3, 6 },
                    { 6, new DateTime(2025, 9, 26, 15, 10, 0, 0, DateTimeKind.Unspecified), true, "Hi Mark! Yes, business accounts are on the roadmap for the second release.", new DateTime(2025, 9, 26, 16, 0, 0, 0, DateTimeKind.Unspecified), 6, 3 },
                    { 7, new DateTime(2025, 11, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), true, "Hi David, is MediTrack available for testing? My father has diabetes and this would help a lot.", new DateTime(2025, 11, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), 4, 7 },
                    { 8, new DateTime(2025, 11, 4, 12, 30, 0, 0, DateTimeKind.Unspecified), true, "Hi Lena, we open the beta next month - I'll add you to the waiting list!", new DateTime(2025, 11, 4, 13, 0, 0, 0, DateTimeKind.Unspecified), 7, 4 },
                    { 9, new DateTime(2025, 11, 4, 13, 5, 0, 0, DateTimeKind.Unspecified), false, "That would be amazing, thank you!", null, 4, 7 },
                    { 10, new DateTime(2025, 12, 29, 18, 0, 0, 0, DateTimeKind.Unspecified), false, "Hey, any update on the GreenCycle pilot locations?", null, 2, 6 }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "IsRead", "Message", "ReferenceId", "ReferenceType", "Title", "Type", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Congratulations! Your startup \"GreenCycle\" has been approved and is now visible to investors.", 1, "Startup", "Startup Approved", 1, 2 },
                    { 2, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Congratulations! Your startup \"PayLink\" has been approved and is now visible to investors.", 2, "Startup", "Startup Approved", 1, 3 },
                    { 3, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Sarah Miller donated 5000.00 EUR to \"GreenCycle\".", 1, "Donation", "Donation Received", 4, 2 },
                    { 4, new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Your startup \"CryptoBoost\" has been rejected. Reason: Unrealistic funding target and insufficient business plan details.", 9, "Startup", "Startup Rejected", 2, 2 },
                    { 5, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Congratulations! \"StayLocal\" has reached its funding target of 25000.00 EUR.", 7, "Startup", "Funding Target Reached", 4, 4 },
                    { 6, new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Your startup \"RideShare BiH\" has been paused by the administrator.", 10, "Startup", "Startup Paused", 3, 3 },
                    { 7, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Sarah Miller commented on your post \"How GreenCycle Started\".", 1, "BlogPost", "New Comment", 5, 2 },
                    { 8, new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Support has answered your ticket \"How do I verify my profile?\".", 1, "SupportTicket", "Support Ticket Answered", 6, 5 },
                    { 9, new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Your report \"Suspicious funding claims\" has been reviewed. Outcome: ActionTaken.", 1, "Report", "Report Resolved", 7, 5 },
                    { 10, new DateTime(2025, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "\"SnackWise\" has been submitted and is awaiting your review.", 8, "Startup", "New Startup Submitted", 0, 1 },
                    { 11, new DateTime(2025, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Scheduled Maintenance on February 15", 3, "Announcement", "New Announcement", 8, 8 },
                    { 12, new DateTime(2025, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Scheduled Maintenance on February 15", 3, "Announcement", "New Announcement", 8, 7 }
                });

            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "Id", "AdminNote", "BlogPostId", "CreatedAt", "Description", "Reason", "ReportedUserId", "ReporterId", "ResolvedAt", "StartupId", "Status", "TargetType" },
                values: new object[] { 3, "Reviewed the chat history. Issued a warning to the reported user.", null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "This user keeps sending me the same promotional message over and over.", "Spam messages in chat", 6, 2, new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 2 });

            migrationBuilder.InsertData(
                table: "Startups",
                columns: new[] { "Id", "AmountRaised", "ApprovedAt", "CategoryId", "CityId", "CompletedAt", "CreatedAt", "Description", "FounderId", "IsActive", "Name", "PlatformFeePercent", "RejectionReason", "StatusId", "TargetAmount", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 12500m, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GreenCycle is a smart recycling platform that rewards households for properly sorted waste. Our smart bins weigh and classify recyclables, and users collect points they can exchange for discounts at local shops. We are looking for funding to produce the first 500 smart bins and launch a pilot program in Sarajevo.", 2, true, "GreenCycle", 5m, null, 3, 50000m, null },
                    { 2, 24300m, new DateTime(2025, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayLink makes instant peer-to-peer payments simple across the Balkans. One app, one QR code, zero hidden fees. We already have a working prototype and partnerships with two regional banks. The funding will be used for security certification and public launch.", 3, true, "PayLink", 5m, null, 3, 80000m, null },
                    { 3, 45500m, new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 6, null, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "MediTrack is a digital health companion for chronic patients. It tracks therapy schedules, connects patients with their doctors and sends alerts when measurements go out of range. The funds will cover clinical validation and integration with hospital systems.", 4, true, "MediTrack", 5m, null, 3, 120000m, null },
                    { 4, 9800m, new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 1, null, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LearnHub is an online learning platform focused on practical IT skills for the local market. Short, project-based courses in local languages with mentorship from industry professionals. Funding goes towards producing 20 new courses and a mobile app.", 2, true, "LearnHub", 5m, null, 3, 30000m, null },
                    { 5, 15200m, new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 11, null, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "FarmSense builds affordable soil and weather sensors for small family farms. Our dashboard tells farmers exactly when to irrigate and fertilize, cutting water usage by up to 40%. We need funding for the second generation of sensors and field testing.", 8, true, "FarmSense", 5m, null, 3, 60000m, null },
                    { 6, 7400m, new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 2, null, new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "QuestForge is an indie game studio working on a story-driven adventure game inspired by Balkan mythology. A playable demo is already available. The funding will finance full production, voice acting and a Steam release.", 3, true, "QuestForge", 5m, null, 3, 40000m, null },
                    { 7, 26000m, new DateTime(2025, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 7, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "StayLocal connects travelers with authentic local experiences hosted by families - cooking classes, guided hikes and traditional crafts workshops. We reached our funding target and are launching in three cities this spring. Thank you to all our supporters!", 4, true, "StayLocal", 5m, null, 6, 25000m, null },
                    { 8, 0m, null, 8, 11, null, new DateTime(2025, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "SnackWise delivers healthy snack boxes to offices on a weekly subscription. Locally sourced, nutritionist-approved and plastic-free packaging. We are raising funds for our first delivery van and a small packing facility.", 8, true, "SnackWise", 5m, null, 2, 20000m, null },
                    { 9, 0m, null, 2, 1, null, new DateTime(2025, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "CryptoBoost promises guaranteed returns through automated cryptocurrency trading strategies powered by proprietary algorithms.", 2, true, "CryptoBoost", 5m, "Unrealistic funding target and insufficient business plan details. Claims of guaranteed returns violate platform standards.", 4, 500000m, null },
                    { 10, 5000m, new DateTime(2025, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 11, 3, null, new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "RideShare BiH is a carpooling platform for daily commuters between Bosnian cities. Drivers share fuel costs, passengers travel cheaper, and everyone reduces their carbon footprint. Currently paused while we resolve licensing requirements.", 3, true, "RideShare BiH", 5m, null, 5, 70000m, null }
                });

            migrationBuilder.InsertData(
                table: "SupportTickets",
                columns: new[] { "Id", "AdminResponse", "AnsweredAt", "ClosedAt", "CreatedAt", "Message", "Status", "Subject", "UserId" },
                values: new object[,]
                {
                    { 1, "Hi Sarah! Profile verification requires a completed profile with a real name and a confirmed email address. I have verified your profile - the badge should be visible now.", new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "I would like to get the verified badge on my profile. What are the requirements and how do I apply?", 1, "How do I verify my profile?", 5 },
                    { 2, null, null, null, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "When I try to upload a second image for my startup, the app shows an error. The first image worked fine.", 0, "Problem uploading startup images", 2 },
                    { 3, "Hi Mark, the reservation is released automatically by your bank within 3-5 business days since the payment did not complete on our side. No amount was captured. Please try again and contact us if the problem repeats.", new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2025, 12, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "I tried to donate 100 EUR to FarmSense yesterday. The payment failed with an error, but my bank shows the amount as reserved.", 1, "Payment failed but money was deducted", 6 },
                    { 4, "Thanks for the suggestion Emma! We added it to our product backlog. Closing this ticket for now.", new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "It would be great if blog posts could be filtered by startup category, the same way startups can.", 2, "Suggestion: category filter for the blog", 3 },
                    { 5, null, null, null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Before I add my own startup I want to understand the fee. Is it charged on every donation or only when the target is reached?", 0, "How is the platform fee calculated?", 7 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 4 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 5 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 6 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 7 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, 8 }
                });

            migrationBuilder.InsertData(
                table: "BlogPostLikes",
                columns: new[] { "Id", "BlogPostId", "CreatedAt", "UserId" },
                values: new object[,]
                {
                    { 6, 4, new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 7, 4, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 3 }
                });

            migrationBuilder.InsertData(
                table: "BlogPosts",
                columns: new[] { "Id", "AuthorId", "Content", "CreatedAt", "ImageData", "IsActive", "SharedFromBlogPostId", "StartupId", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, "It all began when I realized my building had no way to recycle properly. After months of prototyping smart bins in my garage, GreenCycle was born. In this post I want to share the journey so far and what we plan to do with the funding - from producing the first batch of bins to signing up local shops for the rewards program.", new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, 1, "How GreenCycle Started", null },
                    { 2, 3, "Sending money to a friend across the border should not take three days and cost ten euros. PayLink was born out of that frustration. Here is how our instant payment network works and why we believe the region is ready for it.", new DateTime(2025, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, 2, "Why We Built PayLink", null },
                    { 3, 4, "Chronic patients juggle therapies, appointments and measurements every single day. MediTrack puts all of that in one app connected to the doctor's office. We are sharing our clinical pilot results and the roadmap for the next six months.", new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, 3, "MediTrack: Digital Health for Everyone", null },
                    { 5, 2, "A month after our beta launch, 100 students have completed their first course on LearnHub. Here is what we learned from their feedback and how the funding will help us produce twenty new project-based courses.", new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, null, 4, "LearnHub Reaches Its First 100 Students", null }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "BlogPostId", "Content", "CreatedAt", "IsActive", "UserId" },
                values: new object[,]
                {
                    { 6, 4, "Great checklist, saving this for my next campaign.", new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 2 },
                    { 7, 4, "Point four is so true - founders who reply build trust.", new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 3 }
                });

            migrationBuilder.InsertData(
                table: "Donations",
                columns: new[] { "Id", "Amount", "CompletedAt", "CreatedAt", "Message", "StartupId", "Status", "UserId" },
                values: new object[,]
                {
                    { 1, 5000m, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Great idea, good luck!", 1, "Completed", 5 },
                    { 2, 4500m, new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Recycling done right.", 1, "Completed", 6 },
                    { 3, 3000m, new DateTime(2025, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, "Completed", 7 },
                    { 4, 10000m, new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "The region needs this!", 2, "Completed", 6 },
                    { 5, 8000m, new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, "Completed", 7 },
                    { 6, 6300m, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Looking forward to the launch.", 2, "Completed", 5 },
                    { 7, 20000m, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Digital health is the future.", 3, "Completed", 5 },
                    { 8, 15500m, new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, "Completed", 7 },
                    { 9, 10000m, new DateTime(2025, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Proud to support this.", 3, "Completed", 2 },
                    { 10, 5800m, new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Education matters.", 4, "Completed", 6 },
                    { 11, 4000m, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4, "Completed", 7 },
                    { 12, 7200m, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "My parents are farmers - they need this.", 5, "Completed", 5 },
                    { 13, 8000m, new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 5, "Completed", 6 },
                    { 14, 3400m, new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "The demo was amazing!", 6, "Completed", 7 },
                    { 15, 4000m, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 6, "Completed", 5 },
                    { 16, 10000m, new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tourism with a soul.", 7, "Completed", 5 },
                    { 17, 9000m, new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 7, "Completed", 6 },
                    { 18, 7000m, new DateTime(2025, 12, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Can't wait to book an experience.", 7, "Completed", 7 },
                    { 19, 5000m, new DateTime(2025, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Commuting between cities is painful - good luck.", 10, "Completed", 6 },
                    { 20, 500m, null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Another small boost.", 1, "Pending", 5 }
                });

            migrationBuilder.InsertData(
                table: "Favorites",
                columns: new[] { "Id", "CreatedAt", "StartupId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 5 },
                    { 2, new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 5 },
                    { 3, new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5 },
                    { 4, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 6 },
                    { 5, new DateTime(2025, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 6 },
                    { 6, new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 7 },
                    { 7, new DateTime(2025, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 7 },
                    { 8, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2 },
                    { 9, new DateTime(2025, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 2 },
                    { 10, new DateTime(2025, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 7 }
                });

            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "Id", "AdminNote", "BlogPostId", "CreatedAt", "Description", "Reason", "ReportedUserId", "ReporterId", "ResolvedAt", "StartupId", "Status", "TargetType" },
                values: new object[,]
                {
                    { 1, "Confirmed. The startup was rejected for violating platform standards.", null, new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "This startup promises guaranteed returns from crypto trading, which sounds like a scam.", "Suspicious funding claims", null, 5, new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 3, 0 },
                    { 4, "The demo link is available on the startup's detail page. No violation found.", null, new DateTime(2025, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "The startup claims the demo is 'already available' but I could not find any link to it.", "Misleading description", null, 7, new DateTime(2025, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 2, 0 },
                    { 5, null, 4, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "This post seems to promote external investment services rather than platform startups.", "Off-topic promotion", null, 4, null, null, 0, 1 }
                });

            migrationBuilder.InsertData(
                table: "StartupImages",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "ImageData", "IsActive", "IsCover", "IsLogo", "StartupId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 4 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 5 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 6 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 7 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 8 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 9 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, new byte[0], true, true, false, 10 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, new byte[0], true, false, false, 1 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, new byte[0], true, false, false, 2 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, new byte[0], true, false, false, 3 },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, new byte[0], true, false, false, 6 },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 1 },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 2 },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 3 },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 4 },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 5 },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 6 },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 7 },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 8 },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 9 },
                    { 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 0, new byte[0], true, false, true, 10 }
                });

            migrationBuilder.InsertData(
                table: "StartupLikes",
                columns: new[] { "Id", "CreatedAt", "StartupId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 5 },
                    { 2, new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 6 },
                    { 3, new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 7 },
                    { 4, new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 5 },
                    { 5, new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 6 },
                    { 6, new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5 },
                    { 7, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2 },
                    { 8, new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 6 },
                    { 9, new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 5 },
                    { 10, new DateTime(2025, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 7 },
                    { 11, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 5 },
                    { 12, new DateTime(2025, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 6 },
                    { 13, new DateTime(2025, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 7 },
                    { 14, new DateTime(2025, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 2 }
                });

            migrationBuilder.InsertData(
                table: "BlogPostLikes",
                columns: new[] { "Id", "BlogPostId", "CreatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 2, 1, new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 6 },
                    { 3, 1, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7 },
                    { 4, 2, new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 5, 3, new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "BlogPostId", "Content", "CreatedAt", "IsActive", "UserId" },
                values: new object[,]
                {
                    { 1, 1, "Love this idea! When is the pilot starting in Sarajevo?", new DateTime(2025, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 5 },
                    { 2, 1, "Just donated. The rewards program is a smart touch.", new DateTime(2025, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 6 },
                    { 3, 2, "Finally! Cross-border payments here are a nightmare.", new DateTime(2025, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 7 },
                    { 4, 2, "Which banks are you partnering with?", new DateTime(2025, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 5 },
                    { 5, 3, "As a nurse, I can confirm patients desperately need this.", new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 5 },
                    { 8, 5, "Congrats on the milestone! Any plans for design courses?", new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 7 }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "BillingAddress", "BillingCity", "BillingCountry", "BillingState", "BillingZipCode", "CreatedAt", "Currency", "CustomerEmail", "CustomerName", "DonationId", "PaymentMethod", "Status", "StripeCustomerId", "StripePaymentIntentId", "StripeRefundId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 5000m, "Ferhadija 12", "Sarajevo", "Bosnia and Herzegovina", null, "71000", new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "EUR", "investor1@startupba.com", "Sarah Miller", 1, "card", "succeeded", "cus_seed_0000000001", "pi_seed_0000000001", null, new DateTime(2025, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 10000m, "Knez Mihailova 5", "Belgrade", "Serbia", null, "11000", new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "EUR", "investor2@startupba.com", "Mark Johnson", 4, "card", "succeeded", "cus_seed_0000000002", "pi_seed_0000000002", null, new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 20000m, "Ferhadija 12", "Sarajevo", "Bosnia and Herzegovina", null, "71000", new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EUR", "investor1@startupba.com", "Sarah Miller", 7, "card", "succeeded", "cus_seed_0000000003", "pi_seed_0000000003", null, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "Id", "AdminNote", "BlogPostId", "CreatedAt", "Description", "Reason", "ReportedUserId", "ReporterId", "ResolvedAt", "StartupId", "Status", "TargetType" },
                values: new object[] { 2, null, 2, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "The statistics in this post look made up and could mislead investors.", "Inappropriate content", null, 6, null, null, 0, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedByUserId",
                table: "Announcements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostLikes_BlogPostId_UserId",
                table: "BlogPostLikes",
                columns: new[] { "BlogPostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostLikes_UserId",
                table: "BlogPostLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorId",
                table: "BlogPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_SharedFromBlogPostId",
                table: "BlogPosts",
                column: "SharedFromBlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_StartupId",
                table: "BlogPosts",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ReceiverId",
                table: "Chats",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SenderId",
                table: "Chats",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlogPostId",
                table: "Comments",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_StartupId",
                table: "Donations",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_UserId",
                table: "Donations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_StartupId_UserId",
                table: "Favorites",
                columns: new[] { "StartupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId",
                table: "Favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Genders_Name",
                table: "Genders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DonationId",
                table: "Payments",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSettings_Key",
                table: "PlatformSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_BlogPostId",
                table: "Reports",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedUserId",
                table: "Reports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StartupId",
                table: "Reports",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StartupImages_StartupId",
                table: "StartupImages",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupLikes_StartupId_UserId",
                table: "StartupLikes",
                columns: new[] { "StartupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StartupLikes_UserId",
                table: "StartupLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_CategoryId",
                table: "Startups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_CityId",
                table: "Startups",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_FounderId",
                table: "Startups",
                column: "FounderId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_StatusId",
                table: "Startups",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupStatuses_Name",
                table: "StartupStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CityId",
                table: "Users",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GenderId",
                table: "Users",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "BlogPostLikes");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "StartupImages");

            migrationBuilder.DropTable(
                name: "StartupLikes");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Startups");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "StartupStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
