using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsDataApiService.Migrations
{
    public partial class AddStockQuantityToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                nullable: false,
                defaultValue: 0);  // You can set the default value as needed


            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Products",
                nullable: true,
                defaultValue: null);  // You can set the default value as needed
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
            name: "ImageName",
            table: "Products");
        }
    }
}
