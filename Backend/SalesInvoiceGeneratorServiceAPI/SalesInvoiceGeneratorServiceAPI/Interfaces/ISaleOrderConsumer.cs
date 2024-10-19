using SalesOrderInvoiceAPI.Entities;

namespace SalesInvoiceGeneratorServiceAPI.Interfaces
{
    public interface ISaleOrderConsumer
    {
        public void StartListening();

        //public void ProcessMessage(string message);

        public List<ProcessedOrder> GetProcessedSaleOrders();
    }
}
