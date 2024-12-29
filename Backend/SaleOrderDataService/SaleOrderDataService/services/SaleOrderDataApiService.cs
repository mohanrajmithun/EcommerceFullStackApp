using Microsoft.EntityFrameworkCore;
using SaleOrderDataService.Infrastructure;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;

namespace SaleOrderDataService.services
{
    public class SaleOrderDataApiService : ISaleOrderDataService
    {
        private readonly ILogger<SaleOrderDataApiService> logger;
        private readonly AppDbContext appDbContext;
        private readonly ICustomerDataServiceClient CustomerDataServiceClient;
        private readonly IProductDataServiceClient ProductDataServiceClient;


        public SaleOrderDataApiService(AppDbContext appDbContext, ILogger<SaleOrderDataApiService> logger, ICustomerDataServiceClient CustomerDataServiceClient, IProductDataServiceClient ProductDataServiceClient)
        {
            this.logger = logger;
            this.appDbContext = appDbContext;
            this.CustomerDataServiceClient = CustomerDataServiceClient;
            this.ProductDataServiceClient = ProductDataServiceClient;


        }

        public async Task<SaleOrderDTO> GetSaleOrderbyInvoiceNumber(string invoicenumber)
        {
            logger.LogInformation("Fetching the sale order for invoice number: {InvoiceNumber}...", invoicenumber);

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);

            if (saleOrder != null)
            {
                logger.LogInformation("Sale order found for invoice number: {InvoiceNumber}", invoicenumber);

                var productDetails = await appDbContext.SalesOrders
                    .Where(s => s.InvoiceNumber == invoicenumber)
                    .SelectMany(s => s.Products)
                    .Select(productInfo => new
                    {
                        ProductId = productInfo.ProductId,
                        Quantity = productInfo.Quantity // Assuming Quantity is nullable
                    })
                    .ToListAsync();

                List<int> productIds = productDetails.Select(p => p.ProductId).ToList();
                List<int?> quantities = productDetails.Select(p => p.Quantity).ToList();

                SaleOrderDTO saleOrderDTO = new SaleOrderDTO()
                {
                    InvoiceNumber = saleOrder.InvoiceNumber,
                    CustomerId = saleOrder.CustomerId,
                    InvoiceDate = saleOrder.InvoiceDate,
                    NetTotal = saleOrder.NetTotal,
                    Tax = saleOrder.Tax,
                    ProductIDs = productIds,
                    Quantities = quantities.Where(q => q.HasValue).Select(q => q.Value).ToList(),
                    DeliveryAddress = saleOrder.DeliveryAddress,
                    Status = saleOrder.Status
                };

                logger.LogInformation("Sale order DTO created successfully for invoice number: {InvoiceNumber}", invoicenumber);

                return saleOrderDTO;
            }

            logger.LogWarning("Sale order not found for invoice number: {InvoiceNumber}", invoicenumber);
            return null;
        }

        public async Task<IList<SaleOrderDTO>> GetAllSaleOrders()
        {
            logger.LogInformation("Fetching all sale orders...");

            IList<SaleOrder> saleOrders = await appDbContext.SalesOrders.ToListAsync();
            IList<SaleOrderDTO> allSaleOrders = new List<SaleOrderDTO>();

            foreach (SaleOrder saleOrder in saleOrders)
            {
                logger.LogInformation("Processing sale order for invoice number: {InvoiceNumber}", saleOrder.InvoiceNumber);

                Task<SaleOrderDTO> saleOrderDTOTask = GetSaleOrderbyInvoiceNumber(saleOrder.InvoiceNumber);
                SaleOrderDTO saleOrderDTO = await saleOrderDTOTask;
                allSaleOrders.Add(saleOrderDTO);
            }

            logger.LogInformation("All sale orders fetched successfully.");
            return allSaleOrders;
        }

        public async Task<IList<SaleOrder>> GetSaleOrderForCustomer(int customerId)
        {
            logger.LogInformation("Fetching all sale orders for customer with ID: {CustomerId}...", customerId);

            IList<SaleOrder> saleOrders = await appDbContext.SalesOrders.Where(saleorder => saleorder.CustomerId == customerId).ToListAsync();

            logger.LogInformation("Fetched {SaleOrdersCount} sale orders for customer with ID: {CustomerId}", saleOrders.Count, customerId);
            return saleOrders;
        }

        public async Task<decimal> GetOrderTotalbyInvoiceNumber(string invoicenumber)
        {
            logger.LogInformation("Fetching the sale order for invoice number: {InvoiceNumber} to calculate total...", invoicenumber);

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);

            if (saleOrder != null)
            {
                decimal orderTotal = saleOrder.NetTotal;
                logger.LogInformation("Order total for invoice number: {InvoiceNumber} is {OrderTotal}", invoicenumber, orderTotal);
                return orderTotal;
            }

            logger.LogWarning("Sale order not found for invoice number: {InvoiceNumber}", invoicenumber);
            return 0m;
        }

        public async Task<IList<int>> GetProductIdsforInvoice(string invoicenumber)
        {
            logger.LogInformation("Fetching the Product IDs for invoice number: {InvoiceNumber}...", invoicenumber);

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);

            if (saleOrder != null)
            {
                var productIds = await appDbContext.SalesOrders
                    .Where(saleOrder => saleOrder.InvoiceNumber == invoicenumber)
                    .SelectMany(saleOrder => saleOrder.Products)
                    .Select(productInfo => productInfo.ProductId)
                    .ToListAsync();

                logger.LogInformation("Fetched {ProductIdsCount} product IDs for invoice number: {InvoiceNumber}", productIds.Count, invoicenumber);
                return productIds;
            }

            logger.LogWarning("Sale order not found for invoice number: {InvoiceNumber}", invoicenumber);
            return null;
        }

        public async Task<SaleOrder> CreateSaleOrder(SaleOrderDTO saleOrder, string bearertoken)
        {
            logger.LogInformation("Creating a new sale order for customer with ID: {CustomerId}...", saleOrder.CustomerId);

            Customer customer = await CustomerDataServiceClient.GetCustomerById(saleOrder.CustomerId, bearertoken);

            if (customer != null)
            {
                string invoiceNumber = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
                decimal? orderTotal = saleOrder.NetTotal;

                for (int i = 0; i < saleOrder.ProductIDs.Count; i++)
                {
                    int productId = saleOrder.ProductIDs[i];
                    int quantity = saleOrder.Quantities[i];

                    Product validProduct = await ProductDataServiceClient.GetProductbyID(productId, bearertoken);

                    if (validProduct != null)
                    {
                        SalesOrderProductInfo productInfo = new SalesOrderProductInfo()
                        {
                            InvoiceNumber = invoiceNumber,
                            ProductId = productId,
                            Quantity = quantity
                        };

                        appDbContext.Add(productInfo);
                        logger.LogInformation("Added product ID {ProductId} with quantity {Quantity} to sale order.", productId, quantity);
                    }
                    else
                    {
                        saleOrder.ProductIDs.Remove(productId);
                        logger.LogWarning("Invalid product ID {ProductId} for the sale order. Removed from list.", productId);
                    }
                }

                TaxCalculator.SetTaxRate(0.07m);
                decimal taxOnOrder = TaxCalculator.CalculateTax(orderTotal ?? 0m);

                SaleOrder newSaleOrder = new SaleOrder()
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = DateTime.Now,
                    CustomerId = customer.CustomerId,
                    NetTotal = (orderTotal + taxOnOrder) ?? 0m,
                    DeliveryAddress = saleOrder.DeliveryAddress,
                    Tax = taxOnOrder,
                    Status = OrderStatus.Created
                };

                logger.LogInformation("New sale order being created with invoice number: {InvoiceNumber}", invoiceNumber);
                var createdSaleOrder = await appDbContext.SalesOrders.AddAsync(newSaleOrder);
                await appDbContext.SaveChangesAsync();

                if (createdSaleOrder != null)
                {
                    logger.LogInformation("Sale order created successfully with invoice number: {InvoiceNumber}", invoiceNumber);
                    return newSaleOrder;
                }
            }

            logger.LogWarning("Failed to create sale order for customer with ID: {CustomerId}", saleOrder.CustomerId);
            return null;
        }


        public async Task<SaleOrderDTO> AddProductsToSaleOrderAsync(string invoiceNumber, int productid)
        {
            logger.LogInformation($"Attempting to add product {productid} to sale order with invoice number {invoiceNumber}.");

            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                logger.LogInformation($"Sale order with invoice number {invoiceNumber} found.");

                decimal? Ordertotal = saleOrder.NetTotal;
                IList<int> ProductIDs = await GetProductIdsforInvoice(invoiceNumber);

                var IsValidProduct = await ProductDataServiceClient.GetProductbyID(productid, "");

                if ((IsValidProduct != null) && (!ProductIDs.Contains(IsValidProduct.ProductId)))
                {
                    logger.LogInformation($"Product {productid} is valid and not already in the sale order. Adding product to the sale order.");

                    SalesOrderProductInfo productInfo = new SalesOrderProductInfo()
                    {
                        InvoiceNumber = invoiceNumber,
                        ProductId = productid
                    };

                    appDbContext.Add(productInfo);

                    Ordertotal += IsValidProduct.Price;

                    TaxCalculator.SetTaxRate(0.07m);
                    decimal TaxOnOrder = TaxCalculator.CalculateTax(Ordertotal ?? 0m);

                    saleOrder.NetTotal = Ordertotal ?? 0m;
                    saleOrder.Tax = TaxOnOrder;

                    await appDbContext.SaveChangesAsync();

                    logger.LogInformation($"Product {productid} added. Updated order total: {Ordertotal}. Tax: {TaxOnOrder}.");

                }
                else
                {
                    logger.LogWarning($"Product {productid} is either invalid or already in the sale order.");
                }

                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);
            }

            logger.LogWarning($"Sale order with invoice number {invoiceNumber} not found.");
            return null;
        }

        public async Task<SaleOrderDTO> RemoveProductsFromSaleOrderAsync(string invoiceNumber, int productid)
        {
            logger.LogInformation($"Attempting to remove product {productid} from sale order with invoice number {invoiceNumber}.");

            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                logger.LogInformation($"Sale order with invoice number {invoiceNumber} found.");

                decimal? Ordertotal = saleOrder.NetTotal;
                IList<int> ProductIDs = await GetProductIdsforInvoice(invoiceNumber);

                var IsValidProduct = await ProductDataServiceClient.GetProductbyID(productid, "");

                if ((IsValidProduct != null) && (ProductIDs.Contains(IsValidProduct.ProductId)))
                {
                    logger.LogInformation($"Product {productid} found in the sale order. Removing product from the sale order.");

                    var productInfo = await appDbContext.Set<SalesOrderProductInfo>()
                                            .FirstOrDefaultAsync(p => p.InvoiceNumber == invoiceNumber && p.ProductId == productid);

                    appDbContext.Set<SalesOrderProductInfo>().Remove(productInfo);

                    await appDbContext.SaveChangesAsync();

                    Ordertotal -= IsValidProduct.Price;

                    TaxCalculator.SetTaxRate(0.07m);
                    decimal TaxOnOrder = TaxCalculator.CalculateTax(Ordertotal ?? 0m);

                    saleOrder.NetTotal = Ordertotal ?? 0m;
                    saleOrder.Tax = TaxOnOrder;

                    await appDbContext.SaveChangesAsync();

                    logger.LogInformation($"Product {productid} removed. Updated order total: {Ordertotal}. Tax: {TaxOnOrder}.");
                }
                else
                {
                    logger.LogWarning($"Product {productid} is either invalid or not found in the sale order.");
                }

                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);
            }

            logger.LogWarning($"Sale order with invoice number {invoiceNumber} not found.");
            return null;
        }

        public async Task<SaleOrderDTO> UpdateDeliveryAddressAsync(string invoiceNumber, string deliveryAddress)
        {
            logger.LogInformation($"Attempting to update delivery address for sale order with invoice number {invoiceNumber}.");

            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                logger.LogInformation($"Sale order with invoice number {invoiceNumber} found. Updating delivery address.");

                saleOrder.DeliveryAddress = deliveryAddress;
                await appDbContext.SaveChangesAsync();

                logger.LogInformation($"Delivery address updated for sale order with invoice number {invoiceNumber}.");
                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);
            }

            logger.LogWarning($"Sale order with invoice number {invoiceNumber} not found.");
            return null;
        }

        public async Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus)
        {
            logger.LogInformation($"Attempting to update status for sale order with invoice number {invoiceNumber} to {orderStatus}.");

            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                logger.LogInformation($"Sale order with invoice number {invoiceNumber} found. Updating order status.");

                saleOrder.Status = orderStatus;
                await appDbContext.SaveChangesAsync();

                logger.LogInformation($"Order status for sale order with invoice number {invoiceNumber} updated to {orderStatus}.");
                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);
            }

            logger.LogWarning($"Sale order with invoice number {invoiceNumber} not found.");
            return null;
        }

    }
}
