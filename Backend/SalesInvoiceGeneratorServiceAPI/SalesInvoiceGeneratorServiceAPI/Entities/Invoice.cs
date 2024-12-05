using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SalesAPILibrary.Shared_Enums;

namespace SalesInvoiceGeneratorServiceAPI.Entities
{
    public class Invoice
    {
        public Invoice()
        {

            InvoiceDate = DateTime.Now;

            Products = new List<InvoiceProduct>();
        }
        [BsonId] // Maps this property to MongoDB's `_id` field
        [BsonRepresentation(BsonType.ObjectId)] //
        public string Id { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }

        public decimal NetTotal { get; set; }

        public string? DeliveryAddress { get; set; }

        public decimal Tax { get; set; }

        public List<int> ProductIDs { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }   

        public List<InvoiceProduct> Products { get; set; }

        public decimal GrandTotal { get; set; }

    }

    public class InvoiceProduct
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        public ProductColour? productColor { get; set; }

        public ProductSize? productSize { get; set; }

        public int? Quantity { get; set; }  
    }
}
