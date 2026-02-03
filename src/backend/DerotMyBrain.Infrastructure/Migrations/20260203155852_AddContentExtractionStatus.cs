using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentExtractionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContentExtractionCompletedAt",
                table: "Sources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentExtractionError",
                table: "Sources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentExtractionStatus",
                table: "Sources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2); // Completed - existing sources are already processed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentExtractionCompletedAt",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "ContentExtractionError",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "ContentExtractionStatus",
                table: "Sources");
        }
    }
}
