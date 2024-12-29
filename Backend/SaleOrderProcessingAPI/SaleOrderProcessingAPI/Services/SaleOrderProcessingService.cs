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
using Microsoft.Extensions.Logging;

namespace SaleOrderProcessingAPI.Services
{
    public class SaleOrderProcessingService : ISaleOrderProcessingService
    {
        private readonly AppDbContext appDbContext;
        private readonly ISaleOrderDataServiceClient SaleOrderDataServiceClient;
        private readonly IAddressValidationService addressValidationService;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<SaleOrderProcessingService> _logger;
        private const string QueueName = "ProcessedOrdersQueue";
        private static readonly ActivitySource ActivitySource = new ActivitySource("SaleOrderProcessingService");

        public SaleOrderProcessingService(
            AppDbContext appDbContext,
            ISaleOrderDataServiceClient SaleOrderDataServiceClient,
            IAddressValidationService addressValidationService,
            ILogger<SaleOrderProcessingService> logger)
        {
            this.appDbContext = appDbContext;
            this.SaleOrderDataServiceClient = SaleOrderDataServiceClient;
            this.addressValidationService = addressValidationService;
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq", // Use the service name as the hostname
    		    Port = 5672,            // Ensure the correct port is used
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task<IList<SaleOrderDTO>> FetchSaleOrdersAsync()
        {
            _logger.LogInformation("Fetching sale orders...");
            Task<IList<SaleOrderDTO>> saleorderTask = SaleOrderDataServiceClient.GetAllSaleOrders();
            var saleorders = await saleorderTask;
            _logger.LogInformation("Fetched {Count} sale orders.", saleorders.Count);
            return saleorders.Where(orders => orders.Status == OrderStatus.Created).ToList();
        }

        public async Task<List<ProcessedOrder>> ProcessSaleOrderAsync()
        {
            _logger.LogInformation("Starting sale order processing...");
            var saleOrders = await FetchSaleOrdersAsync();
            var processedOrders = new List<ProcessedOrder>();

            foreach (var order in saleOrders)
            {
                _logger.LogInformation("Validating order {InvoiceNumber}...", order.InvoiceNumber);
                bool isOrderValid = await ValidateOrder(order);

                if (isOrderValid)
                {
                    _logger.LogInformation("Order {InvoiceNumber} is valid. Creating processed order...", order.InvoiceNumber);
                    var processedOrder = CreateProcessedOrder(order);
                    processedOrders.Add(processedOrder);
                }

                _logger.LogInformation("Updating order status for {InvoiceNumber} to Processing...", order.InvoiceNumber);
                await SaleOrderDataServiceClient.UpdateOrderStatusAsync(order.InvoiceNumber, OrderStatus.Processing);
            }

            foreach (var processedOrder in processedOrders)
            {
                _logger.LogInformation("Enqueuing processed order {InvoiceNumber}...", processedOrder.InvoiceNumber);
                EnqueueProcessedOrder(processedOrder);
            }

            _logger.LogInformation("Sale order processing completed. {Count} orders processed.", processedOrders.Count);
            return processedOrders;
        }

        public ProcessedOrder CreateProcessedOrder(SaleOrderDTO order)
        {
            _logger.LogInformation("Creating processed order for {InvoiceNumber}...", order.InvoiceNumber);
            var processedProducts = new List<ProcessedProduct>();

            if (order.Quantities.Count == 0)
            {
                _logger.LogWarning("No quantities provided for {InvoiceNumber}. Defaulting to quantity of 1 for all products.", order.InvoiceNumber);
                order.Quantities = Enumerable.Repeat(1, order.ProductIDs.Count).ToList();
            }

            for (int i = 0; i < order.ProductIDs.Count; i++)
            {
                processedProducts.Add(new ProcessedProduct
                {
                    ProductId = order.ProductIDs[i],
                    Quantity = order.Quantities[i]
                });
            }

            _logger.LogInformation("Processed order for {InvoiceNumber} created successfully.", order.InvoiceNumber);
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
            _logger.LogInformation("Validating order {InvoiceNumber}...", order.InvoiceNumber);
            if (order == null || order.ProductIDs == null || !order.ProductIDs.Any())
            {
                _logger.LogWarning("Order {InvoiceNumber} validation failed: Missing products.", order.InvoiceNumber);
                return false;
            }

            bool isAddressValid = await addressValidationService.IsAddressValidAsync(order.DeliveryAddress);
            if (!isAddressValid)
            {
                _logger.LogWarning("Order {InvoiceNumber} validation failed: Invalid delivery address.", order.InvoiceNumber);
                return false;
            }

            _logger.LogInformation("Order {InvoiceNumber} validation succeeded.", order.InvoiceNumber);
            return true;
        }

        public async Task<List<ProcessedOrder>> ProcessShippedCancelledDeliveredOrdersAsync(string invoiceNumber)
        {
            _logger.LogInformation("Processing shipped, cancelled, or delivered orders for {InvoiceNumber}...", invoiceNumber);
            var saleOrders = await FetchShippedCancelledDeliveredOrdersAsync(invoiceNumber);
            var processedOrders = new List<ProcessedOrder>();

            foreach (var order in saleOrders)
            {
                _logger.LogInformation("Validating order {InvoiceNumber}...", order.InvoiceNumber);
                bool isOrderValid = await ValidateOrder(order);

                if (isOrderValid)
                {
                    _logger.LogInformation("Order {InvoiceNumber} is valid. Creating processed order...", order.InvoiceNumber);
                    var processedOrder = CreateProcessedOrder(order);
                    processedOrders.Add(processedOrder);
                }
            }

            foreach (var processedOrder in processedOrders)
            {
                _logger.LogInformation("Enqueuing processed order {InvoiceNumber}...", processedOrder.InvoiceNumber);
                EnqueueProcessedOrder(processedOrder);
            }

            _logger.LogInformation("Processed shipped, cancelled, or delivered orders for {InvoiceNumber} completed.", invoiceNumber);
            return processedOrders;
        }

        public async Task<IList<SaleOrderDTO>> FetchShippedCancelledDeliveredOrdersAsync(string invoiceNumber)
        {
            _logger.LogInformation("Fetching shipped, cancelled, or delivered orders for {InvoiceNumber}...", invoiceNumber);
            Task<IList<SaleOrderDTO>> saleorderTask = SaleOrderDataServiceClient.GetAllSaleOrders();
            var saleorders = await saleorderTask;

            var filteredOrders = saleorders.Where(orders => orders.InvoiceNumber == invoiceNumber).ToList();
            _logger.LogInformation("Fetched {Count} orders for {InvoiceNumber}.", filteredOrders.Count, invoiceNumber);
            return filteredOrders;
        }

        public void EnqueueProcessedOrder(ProcessedOrder order)
        {
            using var activity = ActivitySource.StartActivity("RabbitMQ Publish", ActivityKind.Producer);

            try
            {
                _logger.LogInformation("Publishing order {InvoiceNumber} to RabbitMQ...", order.InvoiceNumber);
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

                _logger.LogInformation("Order {InvoiceNumber} published to RabbitMQ successfully.", order.InvoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order {InvoiceNumber} to RabbitMQ.", order.InvoiceNumber);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.message", ex.Message);
                activity?.SetTag("exception.stacktrace", ex.StackTrace);
                throw;
            }
        }
    }
}
