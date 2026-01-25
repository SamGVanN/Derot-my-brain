using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExploreFieldsToUserActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BacklogAddsCount",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExploreDurationSeconds",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultingReadActivityId",
                table: "Activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ResultingReadActivityId",
                table: "Activities",
                column: "ResultingReadActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ResultingReadActivityId",
                table: "Activities",
                column: "ResultingReadActivityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ResultingReadActivityId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ResultingReadActivityId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "BacklogAddsCount",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ExploreDurationSeconds",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ResultingReadActivityId",
                table: "Activities");
        }
    }
}
