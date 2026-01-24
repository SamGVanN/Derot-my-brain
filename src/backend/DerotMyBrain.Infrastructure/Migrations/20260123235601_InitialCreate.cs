using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastConnectionAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ContentSourceType = table.Column<string>(type: "TEXT", nullable: true),
                    ArticleContent = table.Column<string>(type: "TEXT", nullable: true),
                    LlmModelName = table.Column<string>(type: "TEXT", nullable: true),
                    LlmVersion = table.Column<string>(type: "TEXT", nullable: true),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    BestScore = table.Column<int>(type: "INTEGER", nullable: false),
                    BestScoreDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalQuizAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalReadSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    LastInteraction = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedTopics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "en"),
                    Theme = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "system"),
                    FavoriteCategories = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionsPerQuiz = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    DefaultDifficulty = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_LastAttemptDate",
                table: "Activities",
                columns: new[] { "UserId", "LastAttemptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_Title_LastAttemptDate",
                table: "Activities",
                columns: new[] { "UserId", "Title", "LastAttemptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTopics_UserId_Title",
                table: "TrackedTopics",
                columns: new[] { "UserId", "Title" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "TrackedTopics");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
