using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerotMyBrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshCountToUserActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RefreshCount",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshCount",
                table: "Activities");
        }
    }
}
