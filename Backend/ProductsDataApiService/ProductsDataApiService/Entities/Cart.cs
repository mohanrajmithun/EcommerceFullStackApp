using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SalesAPILibrary.Shared_Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductsDataApiService.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; } // Foreign key to the Customer API
        [ValidateNever]
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalPrice { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
