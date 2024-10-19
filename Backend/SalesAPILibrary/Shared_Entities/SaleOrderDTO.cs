using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SalesAPILibrary.Shared_Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesAPILibrary.Shared_Entities
{
    public class SaleOrderDTO


    {
        public SaleOrderDTO() {

            ProductIDs = new List<int>();
            Quantities = new List<int>();   
        } 
        [ValidateNever]
        public string InvoiceNumber { get; set; }
        [ValidateNever]

        public DateTime InvoiceDate { get; set; }


        public int CustomerId { get; set; }
       

        public decimal NetTotal { get; set; }

        public string? DeliveryAddress { get; set; }

        public decimal? Tax { get; set; }

        public OrderStatus Status { get; set; }

        public List<int> ProductIDs { get; set; }

        public List<int>? Quantities { get; set; }

    }
}
