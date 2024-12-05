using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaleOrderDataService.ServiceClients;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using SalesOrderInvoiceAPI.Entities;

namespace SaleOrderDataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleOrderDataServiceController : Controller
    {
        private readonly ISaleOrderDataService SaleOrderDataService;
        private readonly ISaleOrderProcessingServiceClient saleOrderProcessingServiceClient;


        public SaleOrderDataServiceController(ISaleOrderDataService SaleOrderDataService, ISaleOrderProcessingServiceClient saleOrderProcessingServiceClient)
        {
            this.SaleOrderDataService = SaleOrderDataService;
            this.saleOrderProcessingServiceClient = saleOrderProcessingServiceClient;

        }


        [HttpGet("GetSaleOrder")]

        public async Task<ActionResult<SaleOrderDTO>> GetSaleOrderbyInvoiceNumber(string invoicenumber)
        {
            try
            {
                var SaleOrder = await SaleOrderDataService.GetSaleOrderbyInvoiceNumber(invoicenumber);
                if (SaleOrder != null)
                {
                    return Ok(SaleOrder);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }

        }

        [HttpGet("GetAllSaleOrder")]

        public async Task<ActionResult<IList<SaleOrderDTO>>> GetAllSaleOrders()
        {
            try
            {
                IList<SaleOrderDTO> SaleOrder = await SaleOrderDataService.GetAllSaleOrders();

                if (SaleOrder != null)
                {
                    return Ok(SaleOrder);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }

        }

        [HttpGet("GetOrderTotal")]

        public async Task<ActionResult<decimal>> GetOrderTotalbyInvoiceNumber(string invoicenumber)
        {
            try
            {
                var Ordertotal = await SaleOrderDataService.GetOrderTotalbyInvoiceNumber(invoicenumber);

                if (Ordertotal != null)
                {
                    return Ok(Ordertotal);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [HttpGet("GetSaleOrderforCustomer")]

        public async Task<ActionResult<IList<SaleOrder>>> GetSaleOrderForCustomer(int customerid)
        {
            try
            {
                IList<SaleOrder> SaleOrder = await SaleOrderDataService.GetSaleOrderForCustomer(customerid);

                if (SaleOrder != null)
                {
                    return Ok(SaleOrder);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }
        [HttpGet("GetProductIdsforInvoice")]

        public async Task<ActionResult<IList<int>>> GetProductIdsforInvoice(string invoicenumber)
        {
            try
            {
                IList<int> productIds = await SaleOrderDataService.GetProductIdsforInvoice(invoicenumber);

                if (productIds != null)
                {
                    return Ok(productIds);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }


        [HttpPost("CreateSaleOrder")]

        public async Task<ActionResult<SaleOrder>> CreateSaleOrder(SaleOrderDTO saleOrder)
        {
            try
            {
                var bearerToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                SaleOrder SaleOrder = await SaleOrderDataService.CreateSaleOrder(saleOrder, bearerToken);

                if (SaleOrder != null)
                {
                    return Ok(SaleOrder);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [HttpPost("AddProductsToSaleOrder")]

        public async Task<ActionResult<Task<SaleOrderDTO>>> AddProductsToSaleOrder(string invoiceNumber, int productid)
        {
            try
            {
                SaleOrderDTO SaleOrderDTO = await SaleOrderDataService.AddProductsToSaleOrderAsync(invoiceNumber, productid);

                if (SaleOrderDTO != null)
                {
                    return Ok(SaleOrderDTO);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [HttpDelete("RemoveProductsFromSaleOrder")]

        public async Task<ActionResult<Task<SaleOrderDTO>>> RemoveProductsFromSaleOrder(string invoiceNumber, int productid)
        {
            try
            {
                SaleOrderDTO SaleOrderDTO = await SaleOrderDataService.RemoveProductsFromSaleOrderAsync(invoiceNumber, productid);

                if (SaleOrderDTO != null)
                {
                    return Ok(SaleOrderDTO);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [HttpPatch("UpdateDeliveryAddress")]
        public async Task<ActionResult<SaleOrderDTO>> UpdateDeliveryAddress(string invoiceNumber, string deliveryAddress)
        {
            try
            {
                SaleOrderDTO SaleOrderDTO = await SaleOrderDataService.UpdateDeliveryAddressAsync(invoiceNumber, deliveryAddress);

                if (SaleOrderDTO != null)
                {
                    return Ok(SaleOrderDTO);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }


        [HttpGet("UpdateOrderStatus")]
        public async Task<ActionResult<SaleOrderDTO>> UpdateOrderStatus(string invoiceNumber, OrderStatus orderStatus)
        {
            try
            {
                SaleOrderDTO SaleOrderDTO = await SaleOrderDataService.UpdateOrderStatusAsync(invoiceNumber, orderStatus);

                if (SaleOrderDTO != null)
                {
                    if (orderStatus == OrderStatus.Cancelled | orderStatus == OrderStatus.Shipped | orderStatus == OrderStatus.Delivered)
                    {
                        List<ProcessedOrder> processedOrders = await saleOrderProcessingServiceClient.ProcessShippedCancelledDeliveredOrders(invoiceNumber);
                    }
                    return Ok(SaleOrderDTO);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

    }
}
