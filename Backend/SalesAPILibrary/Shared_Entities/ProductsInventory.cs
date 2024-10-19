using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace SalesAPILibrary.Shared_Entities
{
    public class ProductsInventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        [JsonIgnore]
        public Product Product { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }

        public DateTime LastUpdated { get; set; }

        public ProductsInventory()
        {
            LastUpdated = DateTime.UtcNow;
        }
    }
}
