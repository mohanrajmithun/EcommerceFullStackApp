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
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings, ILogger<InvoiceService> logger)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _invoiceCollection = database.GetCollection<Invoice>("Invoices");
            _logger = logger;
            _logger.LogInformation("InvoiceService initialized with database: {DatabaseName}", settings.Value.DatabaseName);
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            _logger.LogInformation("Fetching all invoices.");
            return await _invoiceCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            _logger.LogInformation("Fetching invoice with InvoiceNumber: {InvoiceNumber}", invoiceNumber);
            return await _invoiceCollection.Find(i => i.InvoiceNumber == invoiceNumber).FirstOrDefaultAsync();
        }

        public async Task CreateInvoiceAsync(Invoice invoice)
        {
            _logger.LogInformation("Creating a new invoice with InvoiceNumber: {InvoiceNumber}", invoice.InvoiceNumber);
            await _invoiceCollection.InsertOneAsync(invoice);
            _logger.LogInformation("Invoice created successfully: {InvoiceNumber}", invoice.InvoiceNumber);
        }

        public async Task<List<Invoice>> GetInvoicesForCustomerAsync(int customerId)
        {
            _logger.LogInformation("Fetching invoices for CustomerId: {CustomerId}", customerId);

            if (customerId <= 0)
            {
                _logger.LogWarning("Invalid CustomerId provided: {CustomerId}", customerId);
                throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));
            }

            var filter = Builders<Invoice>.Filter.Eq(invoice => invoice.CustomerId, customerId);
            var invoices = await _invoiceCollection.Find(filter).ToListAsync();

            _logger.LogInformation("Retrieved {InvoiceCount} invoices for CustomerId: {CustomerId}", invoices.Count, customerId);
            return invoices;
        }

        public string GenerateInvoicePdf(Invoice invoice)
        {
            _logger.LogInformation("Generating PDF for InvoiceNumber: {InvoiceNumber}", invoice.InvoiceNumber);

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, $"{invoice.InvoiceNumber}.pdf");

            try
            {
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdf);

                    document.Add(new Paragraph("INVOICE")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(20));

                    document.Add(new Paragraph($"Invoice Number: {invoice.InvoiceNumber}"));
                    document.Add(new Paragraph($"Invoice Date: {invoice.InvoiceDate:dd/MM/yyyy}"));
                    document.Add(new Paragraph($"Customer Name: {invoice.CustomerName}"));
                    document.Add(new Paragraph($"Delivery Address: {invoice.DeliveryAddress}"));

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

                    document.Add(new Paragraph($"Net Total: {invoice.NetTotal:C}"));
                    document.Add(new Paragraph($"Tax: {invoice.Tax:C}"));
                    document.Add(new Paragraph($"Grand Total: {invoice.GrandTotal:C}"));

                    document.Add(new Paragraph("Thank you for your purchase!").SetTextAlignment(TextAlignment.CENTER));
                }

                _logger.LogInformation("PDF generated successfully at path: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for InvoiceNumber: {InvoiceNumber}", invoice.InvoiceNumber);
                throw;
            }

            return filePath;
        }
    }
}
