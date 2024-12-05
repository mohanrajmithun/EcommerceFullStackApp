using SalesOrderInvoiceAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface ISaleOrderProcessingServiceClient
    {
        Task<List<ProcessedOrder>> ProcessSaleOrdersAsync();
        Task<List<ProcessedOrder>> ProcessShippedCancelledDeliveredOrders(string invoiceNumber);
    }
}
