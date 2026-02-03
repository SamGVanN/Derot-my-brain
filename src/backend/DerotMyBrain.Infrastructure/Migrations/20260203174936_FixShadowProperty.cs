using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_SourceId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_BacklogItems_SourceId",
                table: "BacklogItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsInBacklog",
                table: "Sources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SourceId",
                table: "Documents",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacklogItems_SourceId",
                table: "BacklogItems",
                column: "SourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_SourceId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_BacklogItems_SourceId",
                table: "BacklogItems");

            migrationBuilder.DropColumn(
                name: "IsInBacklog",
                table: "Sources");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SourceId",
                table: "Documents",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogItems_SourceId",
                table: "BacklogItems",
                column: "SourceId");
        }
    }
}
