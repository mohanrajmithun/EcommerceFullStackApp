using Microsoft.AspNetCore.Mvc;
using SaleOrderProcessingAPI.Interfaces;
using SalesOrderInvoiceAPI.Entities;

namespace SaleOrderProcessingAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SaleOrderProcessingController : ControllerBase
    {
        private readonly ISaleOrderProcessingService saleOrderProcessing;
        private readonly ILogger<SaleOrderProcessingController> logger;


        public SaleOrderProcessingController(ISaleOrderProcessingService saleOrderProcessing, ILogger<SaleOrderProcessingController> logger)
        {
            this.saleOrderProcessing = saleOrderProcessing;
            this.logger = logger;


        }

        [HttpGet("ProcessSaleOrders")]
        public async Task<ActionResult<List<ProcessedOrder>>> ProcessSaleOrders()
        {
            logger.LogInformation("processing sale orders...");

            return await saleOrderProcessing.ProcessSaleOrderAsync();



        }

        [HttpGet("ProcessShippedCancelledDeliveredOrders")]
        public async Task<ActionResult<List<ProcessedOrder>>>  ProcessShippedCancelledDeliveredOrders(string invoiceNumber)
        {
            logger.LogInformation("processing sale orders...");

            return await saleOrderProcessing.ProcessShippedCancelledDeliveredOrdersAsync(invoiceNumber);
        }

    }
}
