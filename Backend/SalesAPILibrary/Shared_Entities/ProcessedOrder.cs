namespace SalesOrderInvoiceAPI.Entities
{
    public class ProcessedOrder
    {
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ProcessedProduct> Products { get; set; }
    }

    public class ProcessedProduct
    {
        public int ProductId { get; set; }

        public int? Quantity {  get; set; }


    }
}
