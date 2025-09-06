using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideGreniers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSystemAndFavoriteCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriteCount",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoriteCount",
                table: "Events");
        }
    }
}
