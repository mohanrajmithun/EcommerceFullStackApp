using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SalesAPILibrary.Shared_Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductsDataApiService.Entities
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; } // Foreign key to the Cart
        public virtual Cart Cart { get; set; }
        public int ProductId { get; set; } // Foreign key to the Product API
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Subtotal => Quantity * PricePerUnit; // Calculated field
    }
}
