using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.API.Migrations
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
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    WikipediaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    FirstAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastScore = table.Column<int>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalQuestions = table.Column<int>(type: "INTEGER", nullable: false),
                    LlmModelName = table.Column<string>(type: "TEXT", nullable: true),
                    LlmVersion = table.Column<string>(type: "TEXT", nullable: true),
                    IsTracked = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 10),
                    PreferredTheme = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "derot-brain"),
                    Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "auto"),
                    SelectedCategories = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "idx_activities_tracked",
                table: "Activities",
                columns: new[] { "UserId", "IsTracked" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_type",
                table: "Activities",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_date",
                table: "Activities",
                columns: new[] { "UserId", "LastAttemptDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
