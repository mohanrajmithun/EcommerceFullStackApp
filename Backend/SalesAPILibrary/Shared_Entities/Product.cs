using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SalesAPILibrary.Shared_Enums;

namespace SalesAPILibrary.Shared_Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ProductCode { get; set; } 

        public ProductColour? productColor { get; set; }

        public ProductSize? productSize { get; set; }

        public decimal? Price { get; set; }
        public int CategoryId {  get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        [JsonIgnore]
        public ProductCategory Category { get; set; }

        public string? ImageName { get; set; }

        public int StockQuantity { get; set; }



    }
}
