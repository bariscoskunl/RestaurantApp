using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNumberToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesProducts_AspNetUsers_UserId",
                table: "SalesProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesProducts_Products_ProductId",
                table: "SalesProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesProducts_Sales_SaleId",
                table: "SalesProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalesProducts",
                table: "SalesProducts");

            migrationBuilder.RenameTable(
                name: "SalesProducts",
                newName: "SaleProducts");

            migrationBuilder.RenameIndex(
                name: "IX_SalesProducts_UserId",
                table: "SaleProducts",
                newName: "IX_SaleProducts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesProducts_ProductId",
                table: "SaleProducts",
                newName: "IX_SaleProducts_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "TableNumber",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaleProducts",
                table: "SaleProducts",
                columns: new[] { "SaleId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SaleProducts_AspNetUsers_UserId",
                table: "SaleProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleProducts_Products_ProductId",
                table: "SaleProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleProducts_Sales_SaleId",
                table: "SaleProducts",
                column: "SaleId",
                principalTable: "Sales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleProducts_AspNetUsers_UserId",
                table: "SaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleProducts_Products_ProductId",
                table: "SaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleProducts_Sales_SaleId",
                table: "SaleProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaleProducts",
                table: "SaleProducts");

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "Sales");

            migrationBuilder.RenameTable(
                name: "SaleProducts",
                newName: "SalesProducts");

            migrationBuilder.RenameIndex(
                name: "IX_SaleProducts_UserId",
                table: "SalesProducts",
                newName: "IX_SalesProducts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleProducts_ProductId",
                table: "SalesProducts",
                newName: "IX_SalesProducts_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalesProducts",
                table: "SalesProducts",
                columns: new[] { "SaleId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SalesProducts_AspNetUsers_UserId",
                table: "SalesProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesProducts_Products_ProductId",
                table: "SalesProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesProducts_Sales_SaleId",
                table: "SalesProducts",
                column: "SaleId",
                principalTable: "Sales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
