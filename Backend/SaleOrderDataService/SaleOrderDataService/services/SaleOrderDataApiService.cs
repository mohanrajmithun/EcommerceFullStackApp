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
            logger.LogInformation("Fetching the sale order ...");

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);


            if (saleOrder != null)
            {
                //List<int> quantities = new List<int>();
                var productDetails = await appDbContext.SalesOrders
                    .Where(s => s.InvoiceNumber == invoicenumber)
                    .SelectMany(s => s.Products)
                    .Select(productInfo => new
                    {
                        ProductId = productInfo.ProductId,
                        Quantity = productInfo.Quantity // Assuming Quantity is nullable
                    })
                    .ToListAsync();

                // Split into separate lists for product IDs and quantities
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

                return saleOrderDTO;


            }


            return null;

        }

        public async Task<IList<SaleOrderDTO>> GetAllSaleOrders()
        {
            logger.LogInformation("Fetching all the sale orders ...");

            IList<SaleOrder> saleOrders = await appDbContext.SalesOrders.ToListAsync();
            IList<SaleOrderDTO> allsaleorders = new List<SaleOrderDTO>();

            foreach (SaleOrder saleOrder in saleOrders)
            {
                Task<SaleOrderDTO> saleOrderDTOTask = GetSaleOrderbyInvoiceNumber(saleOrder.InvoiceNumber);

                SaleOrderDTO saleOrderDTO = await saleOrderDTOTask;
                allsaleorders.Add(saleOrderDTO);
            }



            return allsaleorders;


        }


        public async Task<IList<SaleOrder>> GetSaleOrderForCustomer(int customerid)
        {
            logger.LogInformation($"Fetching all the sale orders for customer {customerid} ...");

            IList<SaleOrder> saleOrders = await appDbContext.SalesOrders.Where(saleorder => saleorder.CustomerId == customerid).ToListAsync();


            return saleOrders;
        }


        public async Task<decimal> GetOrderTotalbyInvoiceNumber(string invoicenumber)
        {
            logger.LogInformation("Fetching the sale order ...");

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);

            Decimal Ordertotal = saleOrder.NetTotal;

            return Ordertotal;

        }

        public async Task<IList<int>> GetProductIdsforInvoice(string invoicenumber)
        {

            logger.LogInformation("Fetching the Product Ids ...");

            SaleOrder saleOrder = await appDbContext.SalesOrders.SingleOrDefaultAsync(saleorder => saleorder.InvoiceNumber == invoicenumber);


            if (saleOrder != null)
            {
                var productIds = await appDbContext.SalesOrders
                        .Where(saleOrder => saleOrder.InvoiceNumber == invoicenumber)
                        .SelectMany(saleOrder => saleOrder.Products)
                        .Select(productInfo => productInfo.ProductId)
                        .ToListAsync();

                return productIds;



            }

            return null;

        }


        public async Task<SaleOrder> CreateSaleOrder(SaleOrderDTO saleOrder, string bearertoken)
        {

            Customer customer = await CustomerDataServiceClient.GetCustomerById(saleOrder.CustomerId, bearertoken);

            if (customer != null)
            {
                string InvoiceNumber = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
                decimal? Ordertotal = saleOrder.NetTotal;

                for (int i = 0; i < saleOrder.ProductIDs.Count; i++)
                {
                    int productId = saleOrder.ProductIDs[i];
                    int quantity = saleOrder.Quantities[i];  // Access the corresponding quantity

                    Product IsValidProduct = await ProductDataServiceClient.GetProductbyID(productId, bearertoken);

                    if (IsValidProduct != null)
                    {
                        SalesOrderProductInfo productInfo = new SalesOrderProductInfo()
                        {
                            InvoiceNumber = InvoiceNumber,
                            ProductId = productId,
                            Quantity = quantity  // Assign the corresponding quantity here
                        };

                        appDbContext.Add(productInfo);
                    }
                    else
                    {
                        saleOrder.ProductIDs.Remove(productId);
                    }
                }


                TaxCalculator.SetTaxRate(0.07m);
                decimal TaxOnOrder = TaxCalculator.CalculateTax(Ordertotal ?? 0m);



                SaleOrder new_saleOrder = new SaleOrder()
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = DateTime.Now,
                    CustomerId = customer.CustomerId,
                    NetTotal = Ordertotal + TaxOnOrder ?? 0m,
                    DeliveryAddress = saleOrder.DeliveryAddress,
                    Tax = TaxOnOrder,
                    Status = OrderStatus.Created

                };

                logger.LogInformation("Creating a new SaleOrder");
                var created_saleOrder = await appDbContext.SalesOrders.AddAsync(new_saleOrder);

                await appDbContext.SaveChangesAsync();

                if (created_saleOrder != null)
                {

                    return new_saleOrder;
                }


            }

            return null;

        }

        public async Task<SaleOrderDTO> AddProductsToSaleOrderAsync(string invoiceNumber, int productid)
        {
            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                decimal? Ordertotal = saleOrder.NetTotal;
                IList<int> ProductIDs = await GetProductIdsforInvoice(invoiceNumber);


                var IsValidProduct = await ProductDataServiceClient.GetProductbyID(productid,"");

                if ((IsValidProduct != null) && (!ProductIDs.Contains(IsValidProduct.ProductId)))
                {
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


                }





                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);


            }

            return null;
        }

        public async Task<SaleOrderDTO> RemoveProductsFromSaleOrderAsync(string invoiceNumber, int productid)
        {
            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                decimal? Ordertotal = saleOrder.NetTotal;
                IList<int> ProductIDs = await GetProductIdsforInvoice(invoiceNumber);



                var IsValidProduct = await ProductDataServiceClient.GetProductbyID(productid, "");

                if ((IsValidProduct != null) && (ProductIDs.Contains(IsValidProduct.ProductId)))
                {
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


                }





                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);


            }

            return null;
        }

        public async Task<SaleOrderDTO> UpdateDeliveryAddressAsync(string invoiceNumber, string deliveryAddress)
        {
            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                saleOrder.DeliveryAddress = deliveryAddress;
                await appDbContext.SaveChangesAsync();


                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);

            }

            return null;

        }


        public async Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus)
        {
            SaleOrder saleOrder = await appDbContext.SalesOrders.FirstOrDefaultAsync(SO => SO.InvoiceNumber == invoiceNumber);

            if (saleOrder != null)
            {
                saleOrder.Status = orderStatus;
                await appDbContext.SaveChangesAsync();


                return await GetSaleOrderbyInvoiceNumber(invoiceNumber).ConfigureAwait(false);

            }

            return null;

        }


    }
}
