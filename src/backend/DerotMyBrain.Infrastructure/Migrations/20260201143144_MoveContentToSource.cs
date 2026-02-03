using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveContentToSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextContent",
                table: "Sources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Sources SET TextContent = (SELECT ArticleContent FROM Activities WHERE Activities.SourceId = Sources.Id AND Activities.ArticleContent IS NOT NULL LIMIT 1) WHERE EXISTS (SELECT 1 FROM Activities WHERE Activities.SourceId = Sources.Id AND Activities.ArticleContent IS NOT NULL)");

            migrationBuilder.DropColumn(
                name: "ArticleContent",
                table: "Activities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextContent",
                table: "Sources");

            migrationBuilder.AddColumn<string>(
                name: "ArticleContent",
                table: "Activities",
                type: "TEXT",
                nullable: true);
        }
    }
}
