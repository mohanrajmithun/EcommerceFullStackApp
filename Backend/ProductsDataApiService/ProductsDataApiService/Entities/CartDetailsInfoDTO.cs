using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProductsDataApiService.Migrations;
using SalesAPILibrary.Shared_Entities;

namespace ProductsDataApiService.Entities
{
    public class CartDetailsInfoDTO
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; } // Foreign key to the Customer API
        public List<CartDetailsProductInfo> Products { get; set; } = new List<CartDetailsProductInfo>();
        public decimal TotalPrice { get; set; }
        public string? DeliveryAddress { get; set; }
    }


    public class CartDetailsProductInfo
    {
        public Product product { get; set; }

        public int quantity { get; set; }

        public decimal subtotal { get; set; }
    }
}


