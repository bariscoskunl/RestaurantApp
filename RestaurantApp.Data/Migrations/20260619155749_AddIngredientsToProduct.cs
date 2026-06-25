using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientsToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Products");
        }
    }
}
