using Microsoft.AspNetCore.Mvc;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using SalesOrderInvoiceAPI.Entities;

namespace SalesInvoiceGeneratorServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleOrderConsumerController : Controller
    {
        private readonly ILogger<SaleOrderConsumerController> logger;
        private readonly ISaleOrderConsumer SaleOrderConsumer;
        public SaleOrderConsumerController(ISaleOrderConsumer SaleOrderConsumer, ILogger<SaleOrderConsumerController> logger)
        {
            this.logger = logger;
            this.SaleOrderConsumer = SaleOrderConsumer;

        }

        [HttpGet("GetProcessedSaleOrders")]
        public ActionResult<Task<List<ProcessedOrder>>> GetProcessedSaleOrders()
        {
            List<ProcessedOrder> ProcessedSaleOrders = SaleOrderConsumer.GetProcessedSaleOrders();
            return Ok(ProcessedSaleOrders);
        }

        [HttpGet("DequeSaleOrders")]

        public ActionResult<Task<string>> DequeSaleOrders()
        {
            SaleOrderConsumer.StartListening();
            return Ok("Consuming the sale orders from the queue");
        }
    }
}
