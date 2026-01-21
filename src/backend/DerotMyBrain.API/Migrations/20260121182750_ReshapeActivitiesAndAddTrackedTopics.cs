using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.API.Migrations
{
    /// <inheritdoc />
    public partial class ReshapeActivitiesAndAddTrackedTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_activities_tracked",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "idx_activities_type",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "BestScore",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "FirstAttemptDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IsTracked",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "LastScore",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "LastAttemptDate",
                table: "Activities",
                newName: "SessionDate");

            migrationBuilder.RenameIndex(
                name: "idx_activities_user_date",
                table: "Activities",
                newName: "IX_UserActivity_UserId_SessionDate");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: "Read",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "TotalQuestions",
                table: "Activities",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackedTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    WikipediaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    TrackedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalReadSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalQuizAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstReadDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastReadDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FirstAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BestScore = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalQuestions = table.Column<int>(type: "INTEGER", nullable: true),
                    BestScoreDate = table.Column<DateTime>(type: "TEXT", nullable: true)
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
                name: "IX_UserActivity_UserId_Topic_SessionDate",
                table: "Activities",
                columns: new[] { "UserId", "Topic", "SessionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_UserId_Type_SessionDate",
                table: "Activities",
                columns: new[] { "UserId", "Type", "SessionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTopic_UserId_BestScore",
                table: "TrackedTopics",
                columns: new[] { "UserId", "BestScore" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTopic_UserId_LastAttemptDate",
                table: "TrackedTopics",
                columns: new[] { "UserId", "LastAttemptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTopic_UserId_Topic_Unique",
                table: "TrackedTopics",
                columns: new[] { "UserId", "Topic" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedTopics");

            migrationBuilder.DropIndex(
                name: "IX_UserActivity_UserId_Topic_SessionDate",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_UserActivity_UserId_Type_SessionDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "SessionDate",
                table: "Activities",
                newName: "LastAttemptDate");

            migrationBuilder.RenameIndex(
                name: "IX_UserActivity_UserId_SessionDate",
                table: "Activities",
                newName: "idx_activities_user_date");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "Read");

            migrationBuilder.AlterColumn<int>(
                name: "TotalQuestions",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BestScore",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstAttemptDate",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsTracked",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastScore",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_activities_tracked",
                table: "Activities",
                columns: new[] { "UserId", "IsTracked" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_type",
                table: "Activities",
                columns: new[] { "UserId", "Type" });
        }
    }
}
