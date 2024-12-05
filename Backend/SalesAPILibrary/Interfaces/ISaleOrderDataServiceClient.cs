using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using SalesOrderInvoiceAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface ISaleOrderDataServiceClient
    {
        Task<IList<SaleOrderDTO>> GetAllSaleOrders();

        Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus);

        Task<SaleOrder> CreateSaleOrder(SaleOrderDTO saleOrder, string bearertoken);


    }
}
