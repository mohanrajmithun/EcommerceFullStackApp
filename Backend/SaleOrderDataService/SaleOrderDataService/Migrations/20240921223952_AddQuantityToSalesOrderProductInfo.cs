using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaleOrderDataService.Migrations
{
    public partial class AddQuantityToSalesOrderProductInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "SalesOrderProductInfo",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "SalesOrderProductInfo");
        }
    }
}
