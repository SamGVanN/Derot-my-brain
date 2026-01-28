using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginExploreId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginExploreId",
                table: "Activities",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginExploreId",
                table: "Activities");
        }
    }
}
