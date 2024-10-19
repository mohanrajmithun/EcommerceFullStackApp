using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using SaleOrderProcessingAPI.Infrastructure;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using SaleOrderProcessingAPI.Interfaces;
using SalesOrderInvoiceAPI.Entities;

namespace SaleOrderProcessingAPI.Services
{
    public class SaleOrderProcessingService : ISaleOrderProcessingService
    {
        private readonly AppDbContext appDbContext;
        private readonly ISaleOrderDataServiceClient SaleOrderDataServiceClient;
        private readonly IAddressValidationService addressValidationService;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "ProcessedOrdersQueue";

        public SaleOrderProcessingService(AppDbContext appDbContext, ISaleOrderDataServiceClient SaleOrderDataServiceClient, IAddressValidationService addressValidationService)
        {
            this.appDbContext = appDbContext;
            this.SaleOrderDataServiceClient = SaleOrderDataServiceClient;
            this.addressValidationService = addressValidationService;
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            }; // Update with your RabbitMQ settings
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }




        public async Task<IList<SaleOrderDTO>> FetchSaleOrdersAsync()
        {
            Task<IList<SaleOrderDTO>> saleorderTask = SaleOrderDataServiceClient.GetAllSaleOrders();

            IList<SaleOrderDTO> saleorders = await saleorderTask;

            return saleorders.Where(orders => orders.Status == OrderStatus.Created).ToList();

        }

        public async Task<List<ProcessedOrder>> ProcessSaleOrderAsync()
        {
            var saleOrders = await FetchSaleOrdersAsync();
            var processedOrders = new List<ProcessedOrder>();

            foreach (var order in saleOrders)
            {
                Task<bool> IsOrderValidTask = ValidateOrder(order);

                bool IsOrderValid = await IsOrderValidTask;

                if (IsOrderValid)
                {
                    var ProcessedOrder = CreateProcessedOrder(order);
                    processedOrders.Add(ProcessedOrder);


                }


                var change_status = await SaleOrderDataServiceClient.UpdateOrderStatusAsync(order.InvoiceNumber, OrderStatus.Processing);



            }

            foreach (var processedOrder in processedOrders)
            {
                EnqueueProcessedOrder(processedOrder);
            }

            return processedOrders;

        }

        public ProcessedOrder CreateProcessedOrder(SaleOrderDTO order)
        {
            var processedProducts = new List<ProcessedProduct>();

            if (order.Quantities.Count == 0)
            {
                List<int> Quanities = new List<int>();
                for (int i = 0; i < order.ProductIDs.Count; i++)
                {
                    Quanities.Add(1);

                }

                order.Quantities = Quanities;

            }

            for (int i = 0; i < order.ProductIDs.Count; i++)
            {
                processedProducts.Add(new ProcessedProduct
                {
                    ProductId = order.ProductIDs[i],
                    Quantity = order.Quantities[i] // Access the corresponding quantity
                });
            }
            // Create processed order with necessary details
            return new ProcessedOrder
            {
                InvoiceNumber = order.InvoiceNumber,
                CustomerId = order.CustomerId,
                TotalAmount = order.NetTotal + (order.Tax ?? 0),


                Products = processedProducts
            };
        }


        public async Task<bool> ValidateOrder(SaleOrderDTO order)
        {
            if (order == null || order.ProductIDs == null || !order.ProductIDs.Any())
            {
                return false;
            }
            bool isAddressValid = await addressValidationService.IsAddressValidAsync(order.DeliveryAddress);


            if (!isAddressValid)
            {
                return false;
            }



            // Add more validation logic as needed
            return true;
        }



        public void EnqueueProcessedOrder(ProcessedOrder order)
        {
            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);
        }

    }
}


