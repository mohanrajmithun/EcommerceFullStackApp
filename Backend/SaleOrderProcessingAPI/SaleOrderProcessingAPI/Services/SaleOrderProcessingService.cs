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
using OpenTelemetry.Trace;
using System.Diagnostics;


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
        private static readonly ActivitySource ActivitySource = new ActivitySource("SaleOrderProcessingService");

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

        public async Task<List<ProcessedOrder>> ProcessShippedCancelledDeliveredOrdersAsync(string invoiceNumber)
        {
            // Fetch orders that are either shipped, cancelled, or delivered
            var saleOrders = await FetchShippedCancelledDeliveredOrdersAsync(invoiceNumber);
            var processedOrders = new List<ProcessedOrder>();

            foreach (var order in saleOrders)
            {
                // Example logic for processing orders (this might vary based on your business rules)
                Task<bool> IsOrderValidTask = ValidateOrder(order);
                bool IsOrderValid = await IsOrderValidTask;

                if (IsOrderValid)
                {
                    var processedOrder = CreateProcessedOrder(order);
                    processedOrders.Add(processedOrder);
                }

                // Update order status based on your business needs
                // This can be "Shipped", "Cancelled", or "Delivered" status update, depending on the requirement
                //var changeStatus = await SaleOrderDataServiceClient.UpdateOrderStatusAsync(order.InvoiceNumber, OrderStatus.Completed);  // Or other status as needed
            }

            // Enqueue processed orders to the RabbitMQ queue
            foreach (var processedOrder in processedOrders)
            {
                EnqueueProcessedOrder(processedOrder);
            }

            return processedOrders;
        }

        // Fetch orders that are shipped, cancelled, or delivered
        public async Task<IList<SaleOrderDTO>> FetchShippedCancelledDeliveredOrdersAsync(string invoiceNumber)
        {
            // Assuming you want orders in Shipped, Cancelled, or Delivered status
            Task<IList<SaleOrderDTO>> saleorderTask = SaleOrderDataServiceClient.GetAllSaleOrders();

            IList<SaleOrderDTO> saleorders = await saleorderTask;

            return saleorders.Where(orders => orders.InvoiceNumber == invoiceNumber).ToList();
        }




        public void EnqueueProcessedOrder(ProcessedOrder order)
        {
            using var activity = ActivitySource.StartActivity("RabbitMQ Publish", ActivityKind.Producer);


            try
            {
                var message = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);

                if (activity != null)
                {
                    activity.SetTag("messaging.system", "rabbitmq");
                    activity.SetTag("messaging.destination_kind", "queue");
                    activity.SetTag("messaging.rabbitmq.queue", QueueName);
                    activity.SetTag("messaging.message_payload_size_bytes", body.Length);
                    activity.SetTag("order.id", order.InvoiceNumber);
                    activity.SetTag("order.totalAmount", order.TotalAmount);
                    activity.SetStatus(ActivityStatusCode.Ok);
                }
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.message", ex.Message);
                activity?.SetTag("exception.stacktrace", ex.StackTrace);
                throw;
            }
        }

    }
}


