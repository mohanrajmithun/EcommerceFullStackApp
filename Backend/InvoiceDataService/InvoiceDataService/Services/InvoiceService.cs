using InvoiceDataService.Infrastructure;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Reflection.Metadata;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;



namespace InvoiceDataService.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IMongoCollection<Invoice> _invoiceCollection;

        public InvoiceService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _invoiceCollection = database.GetCollection<Invoice>("Invoices");
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _invoiceCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _invoiceCollection.Find(i => i.InvoiceNumber == invoiceNumber).FirstOrDefaultAsync();
        }

        public async Task CreateInvoiceAsync(Invoice invoice)
        {
            await _invoiceCollection.InsertOneAsync(invoice);
        }


        public async Task<List<Invoice>> GetInvoicesForCustomerAsync(int customerId)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));
            }

            // Query MongoDB to find invoices for the specific customerId
            var filter = Builders<Invoice>.Filter.Eq(invoice => invoice.CustomerId, customerId);

            // Retrieve the invoices asynchronously
            var invoices = await _invoiceCollection.Find(filter).ToListAsync();

            return invoices;

        }


        public string GenerateInvoicePdf(Invoice invoice)
        {
            // Define the file path
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
            Directory.CreateDirectory(directoryPath); // Ensure the directory exists
            string filePath = Path.Combine(directoryPath, $"{invoice.InvoiceNumber}.pdf");

            // Create the PDF document
            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);

                // Add Invoice Header
                document.Add(new Paragraph("INVOICE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                // Add Customer Information
                document.Add(new Paragraph($"Invoice Number: {invoice.InvoiceNumber}"));
                document.Add(new Paragraph($"Invoice Date: {invoice.InvoiceDate:dd/MM/yyyy}"));
                document.Add(new Paragraph($"Customer Name: {invoice.CustomerName}"));
                document.Add(new Paragraph($"Delivery Address: {invoice.DeliveryAddress}"));

                // Add Order Details
                document.Add(new Paragraph("Order Details").SetBold().SetUnderline());
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 1, 1 }))
                    .UseAllAvailableWidth();
                table.AddHeaderCell("Product Name");
                table.AddHeaderCell("Size");
                table.AddHeaderCell("Color");
                table.AddHeaderCell("Price");
                table.AddHeaderCell("Quantity");

                foreach (var product in invoice.Products)
                {
                    table.AddCell(product.ProductName);
                    table.AddCell(product.productSize.ToString());
                    table.AddCell(product.productColor.ToString());
                    table.AddCell(product.Price.ToString("C"));
                    table.AddCell(product.Quantity.ToString());
                }

                document.Add(table);

                // Add Totals
                document.Add(new Paragraph($"Net Total: {invoice.NetTotal:C}"));
                document.Add(new Paragraph($"Tax: {invoice.Tax:C}"));
                document.Add(new Paragraph($"Grand Total: {invoice.GrandTotal:C}"));

                // Add Footer
                document.Add(new Paragraph("Thank you for your purchase!").SetTextAlignment(TextAlignment.CENTER));
            }

            // Return the file path
            return filePath;
        }



    }

}
