using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;

namespace SalesInvoiceGeneratorServiceAPI.Interfaces
{
    public interface ISaleOrderDataServiceClient
    {
        Task<IList<SaleOrderDTO>> GetAllSaleOrders();
        Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus);
        Task<SaleOrderDTO> GetSaleOrderbyInvoiceNumber(string invoiceNumber);
    }
}
