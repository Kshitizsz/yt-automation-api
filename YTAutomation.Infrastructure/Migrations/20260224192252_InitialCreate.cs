using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YTAutomation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NicheCategory = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrendingTopics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AISource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrendScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketInsights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YoutubeChannelId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VideoJobId = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledPosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VideoJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Script = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoiceoverUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeVideoId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NicheCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledPostId = table.Column<int>(type: "int", nullable: true),
                    ScheduledPostId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoJobs_ScheduledPosts_ScheduledPostId1",
                        column: x => x.ScheduledPostId1,
                        principalTable: "ScheduledPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VideoJobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoJobId = table.Column<int>(type: "int", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    Likes = table.Column<long>(type: "bigint", nullable: false),
                    Comments = table.Column<long>(type: "bigint", nullable: false),
                    Subscribers = table.Column<long>(type: "bigint", nullable: false),
                    WatchTimeMinutes = table.Column<double>(type: "float", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalytics_VideoJobs_VideoJobId",
                        column: x => x.VideoJobId,
                        principalTable: "VideoJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPosts_UserId",
                table: "ScheduledPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPosts_VideoJobId",
                table: "ScheduledPosts",
                column: "VideoJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_VideoJobId",
                table: "VideoAnalytics",
                column: "VideoJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoJobs_ScheduledPostId1",
                table: "VideoJobs",
                column: "ScheduledPostId1");

            migrationBuilder.CreateIndex(
                name: "IX_VideoJobs_UserId",
                table: "VideoJobs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledPosts_VideoJobs_VideoJobId",
                table: "ScheduledPosts",
                column: "VideoJobId",
                principalTable: "VideoJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledPosts_Users_UserId",
                table: "ScheduledPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoJobs_Users_UserId",
                table: "VideoJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledPosts_VideoJobs_VideoJobId",
                table: "ScheduledPosts");

            migrationBuilder.DropTable(
                name: "MarketInsights");

            migrationBuilder.DropTable(
                name: "VideoAnalytics");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VideoJobs");

            migrationBuilder.DropTable(
                name: "ScheduledPosts");
        }
    }
}
