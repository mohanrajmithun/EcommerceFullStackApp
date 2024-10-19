using SalesAPILibrary.Shared_Enums;
using System.ComponentModel.DataAnnotations;

namespace SalesAPILibrary.Shared_Entities
{
    public class ProductDetails
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public ProductColour? productColor { get; set; }

        public ProductSize? productSize { get; set; }

        public decimal? Price { get; set; }
        public string Category { get; set; }

        public int StockQuantity { get; set; }
    }
}
