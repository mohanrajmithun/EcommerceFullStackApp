using InvoiceDataService.Services;
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
        private readonly ILogger<InvoicesController> _logger;


        public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;

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
            _logger.LogInformation("Received request to fetch or generate PDF for InvoiceNumber: {InvoiceNumber}", invoiceNumber);

            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
                string filePath = Path.Combine(directoryPath, $"{invoiceNumber}.pdf");

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogInformation("PDF not found for InvoiceNumber: {InvoiceNumber}, generating new PDF.", invoiceNumber);

                    Invoice invoice = await _invoiceService.GetInvoiceByNumberAsync(invoiceNumber);

                    if (invoice == null)
                    {
                        _logger.LogWarning("No invoice found for InvoiceNumber: {InvoiceNumber}", invoiceNumber);
                        return NotFound($"Invoice with number {invoiceNumber} not found.");
                    }

                    Directory.CreateDirectory(directoryPath);
                    _invoiceService.GenerateInvoicePdf(invoice);
                }
                else
                {
                    _logger.LogInformation("PDF already exists for InvoiceNumber: {InvoiceNumber}", invoiceNumber);
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = $"{invoiceNumber}.pdf";

                _logger.LogInformation("Returning PDF file for InvoiceNumber: {InvoiceNumber}", invoiceNumber);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for InvoiceNumber: {InvoiceNumber}", invoiceNumber);
                return StatusCode(500, $"Error generating or retrieving PDF: {ex.Message}");
            }
        }







    }

}
