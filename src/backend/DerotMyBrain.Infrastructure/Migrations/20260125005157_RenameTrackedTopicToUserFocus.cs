using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTrackedTopicToUserFocus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedTopics");

            migrationBuilder.DropIndex(
                name: "IX_Activities_UserId_LastAttemptDate",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_UserId_Title_LastAttemptDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ContentSourceType",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "SourceUrl",
                table: "Activities",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "MaxScore",
                table: "Activities",
                newName: "QuestionCount");

            migrationBuilder.RenameColumn(
                name: "LastAttemptDate",
                table: "Activities",
                newName: "SessionDateStart");

            migrationBuilder.DropColumn(
                name: "IsTracked",
                table: "Activities");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "SourceHash",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ScorePercentage",
                table: "Activities",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionDateEnd",
                table: "Activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuizDurationSeconds",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadDurationSeconds",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewBestScore",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserFocuses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceHash = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayTitle = table.Column<string>(type: "TEXT", nullable: false),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<double>(type: "REAL", nullable: false),
                    LastScore = table.Column<double>(type: "REAL", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalReadTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalQuizTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalStudyTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFocuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFocuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_SessionDateEnd",
                table: "Activities",
                columns: new[] { "UserId", "SessionDateEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_SessionDateStart",
                table: "Activities",
                columns: new[] { "UserId", "SessionDateStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId_SourceHash",
                table: "Activities",
                columns: new[] { "UserId", "SourceHash" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFocuses_UserId_SourceHash",
                table: "UserFocuses",
                columns: new[] { "UserId", "SourceHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFocuses");

            migrationBuilder.DropIndex(
                name: "IX_Activities_UserId_SessionDateEnd",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_UserId_SessionDateStart",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_UserId_SourceHash",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsNewBestScore",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "QuizDurationSeconds",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ReadDurationSeconds",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SessionDateEnd",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SourceHash",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ScorePercentage",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "Activities",
                newName: "SourceUrl");

            migrationBuilder.RenameColumn(
                name: "QuestionCount",
                table: "Activities",
                newName: "MaxScore");

            migrationBuilder.RenameColumn(
                name: "SessionDateStart",
                table: "Activities",
                newName: "LastAttemptDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsTracked",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "ContentSourceType",
                table: "Activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackedTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    BestScore = table.Column<int>(type: "INTEGER", nullable: false),
                    BestScoreDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastInteraction = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TotalQuizAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalReadSessions = table.Column<int>(type: "INTEGER", nullable: false)
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
    }
}
