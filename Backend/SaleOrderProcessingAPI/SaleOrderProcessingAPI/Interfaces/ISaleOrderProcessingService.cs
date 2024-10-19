using SalesAPILibrary.Shared_Entities;
using SalesOrderInvoiceAPI.Entities;
using System.Threading.Tasks;

namespace SaleOrderProcessingAPI.Interfaces
{
    public interface ISaleOrderProcessingService
    {
        Task<IList<SaleOrderDTO>> FetchSaleOrdersAsync();

        Task<List<ProcessedOrder>> ProcessSaleOrderAsync();
        ProcessedOrder CreateProcessedOrder(SaleOrderDTO order);

        Task<bool> ValidateOrder(SaleOrderDTO order);

    }
}
