using Microsoft.AspNetCore.Mvc;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;

namespace InvoiceMongoDBService.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("{InvoiceNumber}")]
        public async Task<IActionResult> GetInvoice(string InvoiceNumber)
        {
            var invoice = await _invoiceService.GetInvoiceByNumberAsync(InvoiceNumber);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice(Invoice invoice)
        {
            await _invoiceService.CreateInvoiceAsync(invoice);
            return CreatedAtAction(nameof(GetInvoice), new { InvoiceNumber = invoice.InvoiceNumber }, invoice);
        }


        [HttpGet("InvoicesByCustomer")]
        public async Task<ActionResult<List<Invoice>>> GetInvoicesForCustomer(int customerId)
        {
            var invoices = await _invoiceService.GetInvoicesForCustomerAsync(customerId);
            return invoices;


        }


        [HttpGet("GetInvoiceAsPDF")]
        public async Task<IActionResult> GetInvoiceAsPDF(string invoiceNumber)
        {
            try
            {
                // Define the file path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
                string filePath = Path.Combine(directoryPath, $"{invoiceNumber}.pdf");

                // Check if the file already exists
                if (!System.IO.File.Exists(filePath))
                {
                    // Retrieve invoice details (mock or actual implementation)
                    Invoice invoice = await _invoiceService.GetInvoiceByNumberAsync(invoiceNumber); // Replace with your logic

                    // Ensure directory exists
                    Directory.CreateDirectory(directoryPath);

                    // Generate the PDF and save it to the file path
                    _invoiceService.GenerateInvoicePdf(invoice);
                }

                // Return the physical file
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = $"{invoiceNumber}.pdf";

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating or retrieving PDF: {ex.Message}");
            }
        }








    }

}
