using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SalesAPILibrary.Shared_Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SalesAPILibrary.Shared_Entities
{
    public class SaleOrder
    {
        [Key]
        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }


        public int CustomerId { get; set; }
        [ValidateNever]
        [JsonIgnore]

        public ICollection<SalesOrderProductInfo> Products { get; set; }

        public decimal NetTotal { get; set; }   

        public string? DeliveryAddress { get; set; }    

        public decimal? Tax {  get; set; }

        public OrderStatus Status { get; set; }
        [ForeignKey("CustomerId")]
        [ValidateNever]
        [JsonIgnore]


        public Customer Customer { get; set; }

    }
}
