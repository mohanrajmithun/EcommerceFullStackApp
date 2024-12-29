using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MailKit.Net.Smtp;
using MimeKit;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using SalesAPILibrary.Shared_Entities;

namespace SalesInvoiceGeneratorServiceAPI.services
{
    public class InvoicePdfGeneratorService : IInvoicePdfGeneratorService
    {
        private readonly ILogger<InvoicePdfGeneratorService> logger;
        private readonly IConfiguration configuration;

        public InvoicePdfGeneratorService(ILogger<InvoicePdfGeneratorService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task GenerateInvoicePdf(Invoice invoice)
        {
            logger.LogInformation("Generating PDF for Invoice: {InvoiceNumber}", invoice.InvoiceNumber);

            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");
                if (!Directory.Exists(directoryPath))
                {
                    logger.LogInformation("Creating directory for invoices at {DirectoryPath}", directoryPath);
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"{invoice.InvoiceNumber}.pdf");

                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);

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

                logger.LogInformation("Invoice PDF generated successfully at {FilePath}", filePath);

                await SendInvoiceEmail(invoice, filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while generating PDF for Invoice: {InvoiceNumber}", invoice.InvoiceNumber);
                throw;
            }
        }

        private async Task SendInvoiceEmail(Invoice invoice, string filePath)
        {
            logger.LogInformation("Sending invoice email to {CustomerEmail} for Invoice: {InvoiceNumber}", invoice.CustomerEmail, invoice.InvoiceNumber);

            try
            {
                var apiKey = configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("mohanmithun@proton.me", "Shopping Pods");
                var to = new EmailAddress(invoice.CustomerEmail, invoice.CustomerName);
                var subject = $"Your Invoice: {invoice.InvoiceNumber}";
                var plainTextContent = $"Dear {invoice.CustomerName},\n\nPlease find attached your invoice for the order placed.\n\nBest regards,\nYour Sales Team";
                var htmlContent = plainTextContent.Replace("\n", "<br>");

                var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var bytes = File.ReadAllBytes(filePath);
                message.AddAttachment(Path.GetFileName(filePath), Convert.ToBase64String(bytes), "application/pdf");

                var response = await client.SendEmailAsync(message);
                logger.LogInformation("Invoice email sent to {CustomerEmail} with status: {StatusCode}", invoice.CustomerEmail, response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending invoice email to {CustomerEmail} for Invoice: {InvoiceNumber}", invoice.CustomerEmail, invoice.InvoiceNumber);
                throw;
            }
        }

        public async Task SendOrderStatusUpdateEmail(string customerEmail, string orderStatus, string invoiceNumber)
        {
            logger.LogInformation("Sending order status update email to {CustomerEmail} for Invoice: {InvoiceNumber} with status: {OrderStatus}", customerEmail, invoiceNumber, orderStatus);

            try
            {
                var apiKey = configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("mohanmithun@proton.me", "Shopping Pods");
                var to = new EmailAddress(customerEmail);
                var subject = $"Order Status Update: {invoiceNumber}";
                var plainTextContent = $"Dear Customer,\n\nYour order with Invoice Number: {invoiceNumber} has been {orderStatus}.\n\nBest regards,\nYour Sales Team";
                var htmlContent = plainTextContent.Replace("\n", "<br>");

                var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(message);
                logger.LogInformation("Order status update email sent to {CustomerEmail} with status: {StatusCode}", customerEmail, response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending order status update email to {CustomerEmail} for Invoice: {InvoiceNumber}", customerEmail, invoiceNumber);
                throw;
            }
        }
    }
}
