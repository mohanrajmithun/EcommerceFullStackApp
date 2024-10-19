using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MailKit.Net.Smtp;
using MimeKit;
using SalesInvoiceGeneratorServiceAPI.Entities;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SalesInvoiceGeneratorServiceAPI.services
{
    public class InvoicePdfGeneratorService : IInvoicePdfGeneratorService
    {
        private readonly ILogger<InvoicePdfGeneratorService> logger;

        public InvoicePdfGeneratorService(ILogger<InvoicePdfGeneratorService> logger)
        {
            this.logger = logger;
        }

        // Add the async modifier since we're using async methods
        public async Task GenerateInvoicePdf(Invoice invoice)
        {
            // Ensure the "Invoices" directory exists
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Define the path for the PDF file
            string filePath = Path.Combine(directoryPath, $"{invoice.InvoiceNumber}.pdf");

            // Create the PDF document
            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            {
                var document = new Document(pdf);

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

            Console.WriteLine($"Invoice generated at: {filePath}");

            // Send the invoice via email
            await SendInvoiceEmail(invoice, filePath);
        }

        // Since this is an async method, it requires a body and Task as a return type.
        private async Task SendInvoiceEmail(Invoice invoice, string filePath)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("mohanmithun@proton.me", "Shopping Pods");
            var to = new EmailAddress(invoice.CustomerEmail, invoice.CustomerName);
            var subject = $"Your Invoice: {invoice.InvoiceNumber}";
            var plainTextContent = $"Dear {invoice.CustomerName},\n\nPlease find attached your invoice for the order placed.\n\nBest regards,\nYour Sales Team";
            var htmlContent = plainTextContent.Replace("\n", "<br>");

            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Attach the PDF invoice
            var bytes = File.ReadAllBytes(filePath);
            message.AddAttachment(Path.GetFileName(filePath), Convert.ToBase64String(bytes), "application/pdf");

            var response = await client.SendEmailAsync(message);
            Console.WriteLine($"Invoice sent to {invoice.CustomerEmail} with response status: {response.StatusCode}");
        }
    }
}
