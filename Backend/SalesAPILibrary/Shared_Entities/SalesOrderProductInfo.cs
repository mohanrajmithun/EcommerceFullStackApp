using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SalesAPILibrary.Shared_Entities
{
    public class SalesOrderProductInfo
    {
        public SalesOrderProductInfo() { }
        [Key]

        public int ProductId { get; set; }
        [ValidateNever]
        [JsonIgnore]

        public Product Product { get; set; }
        [Key]

        public string InvoiceNumber { get; set; }
        [ValidateNever]
        [JsonIgnore]

        public SaleOrder SaleOrder { get; set; }

        public int? Quantity { get; set; }


    }
}
