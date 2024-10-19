using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface ISaleOrderDataService
    {

        Task<SaleOrderDTO> GetSaleOrderbyInvoiceNumber(string invoicenumber);

        Task<IList<SaleOrderDTO>> GetAllSaleOrders();

        Task<decimal> GetOrderTotalbyInvoiceNumber(string invoicenumber);

        Task<IList<SaleOrder>> GetSaleOrderForCustomer(int customerid);
        Task<IList<int>> GetProductIdsforInvoice(string invoicenumber);

        Task<SaleOrder> CreateSaleOrder(SaleOrderDTO saleOrder, string bearertoken);

        Task<SaleOrderDTO> AddProductsToSaleOrderAsync(string invoiceNumber, int productid);

        Task<SaleOrderDTO> RemoveProductsFromSaleOrderAsync(string invoiceNumber, int productid);

        Task<SaleOrderDTO> UpdateDeliveryAddressAsync(string invoiceNumber, string deliveryAddress);

        Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus);



    }
}
