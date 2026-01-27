using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorActivityDurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExploreDurationSeconds",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "QuizDurationSeconds",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ReadDurationSeconds",
                table: "Activities");

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Activities");

            migrationBuilder.AddColumn<int>(
                name: "ExploreDurationSeconds",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

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
        }
    }
}
