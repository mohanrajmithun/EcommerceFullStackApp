using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using SalesOrderInvoiceAPI.Entities;
using SalesInvoiceGeneratorServiceAPI.Entities;
using System.Text;
using System.Text.Json;
using InvoiceProduct = SalesAPILibrary.Shared_Entities.InvoiceProduct;
using SalesInvoiceGeneratorServiceAPI.ServiceClients;
using ISaleOrderDataServiceClient = SalesInvoiceGeneratorServiceAPI.Interfaces.ISaleOrderDataServiceClient;
using System.Diagnostics;
using OpenTelemetry.Trace;
using SalesAPILibrary.Shared_Enums;
using Invoice = SalesAPILibrary.Shared_Entities.Invoice;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SalesInvoiceGeneratorServiceAPI.services
{
    public class SaleOrderConsumer : ISaleOrderConsumer
    {
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "ProcessedOrdersQueue";
        private readonly List<ProcessedOrder> processedOrders = new List<ProcessedOrder>();
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<SaleOrderConsumer> _logger;
        private static readonly ActivitySource activitySource = new("SalesInvoiceGeneratorServiceAPI");
        private readonly AsyncRetryPolicy _retryPolicy;


        public SaleOrderConsumer(IServiceProvider serviceProvider, ILogger<SaleOrderConsumer> logger)
        {
            this.serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            }; // RabbitMQ settings

            // Define Polly Retry Policy for RabbitMQ connection
            var connectionRetryPolicy = Policy
                .Handle<Exception>() // Handle any exception
                .WaitAndRetry(
                    retryCount: 5, // Number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retrying RabbitMQ connection. Attempt {RetryCount} after {TimeSpan}.", retryCount, timeSpan);
                    });

            // Attempt to connect with retry logic
            connectionRetryPolicy.Execute(() =>
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            });

            _logger.LogInformation("RabbitMQ connection and channel initialized.");

            // Define Polly Retry Policy for processing messages
            _retryPolicy = Policy
                .Handle<Exception>() // Handle any exception
                .WaitAndRetryAsync(
                    retryCount: 3, // Number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retry {RetryCount} after {TimeSpan}.", retryCount, timeSpan);
                    });
        }

        public void StartListening()
        {
            _logger.LogInformation("Starting to listen to queue: {QueueName}", QueueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Message received from queue: {QueueName}, Body: {Message}", QueueName, message);

                using var activity = activitySource.StartActivity("RabbitMQ Consume", ActivityKind.Consumer);
                if (activity != null)
                {
                    activity.SetTag("messaging.system", "rabbitmq");
                    activity.SetTag("messaging.destination_kind", "queue");
                    activity.SetTag("messaging.rabbitmq.queue", QueueName);
                    activity.SetTag("messaging.message_payload_size_bytes", body.Length);
                    activity.SetTag("message.body", message);
                }

                ProcessMessage(message);
            };

            _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
            _logger.LogInformation("Listening to queue: {QueueName}", QueueName);
        }

        private void ProcessMessage(string message)
        {
            _logger.LogInformation("Processing message: {Message}", message);

            using var activity = activitySource.StartActivity("Process Sale Order");
            try
            {
                var processedOrder = JsonSerializer.Deserialize<ProcessedOrder>(message);
                activity?.SetTag("order.id", processedOrder?.InvoiceNumber);

                DequeSaleOrder(processedOrder);
                GenerateInvoiceSaleOrder(processedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
            }
        }

        private async void DequeSaleOrder(ProcessedOrder order)
        {
            _logger.LogInformation("Dequeuing order with InvoiceNumber: {InvoiceNumber}", order.InvoiceNumber);

            using var activity = activitySource.StartActivity("Deque Sale Order");
            try
            {
                if (activity != null)
                {
                    activity.SetTag("order.id", order.InvoiceNumber);
                    activity.SetTag("messaging.system", "rabbitmq");
                    activity.SetTag("messaging.operation", "receive");
                }

                processedOrders.Add(order);
                _logger.LogInformation("Order with InvoiceNumber: {InvoiceNumber} dequeued successfully.", order.InvoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dequeuing order with InvoiceNumber: {InvoiceNumber}", order.InvoiceNumber);
            }
        }

        public List<ProcessedOrder> GetProcessedSaleOrders()
        {
            _logger.LogInformation("Retrieving processed sale orders. Count: {Count}", processedOrders.Count);
            return processedOrders;
        }

        private async void GenerateInvoiceSaleOrder(ProcessedOrder order)
        {
            _logger.LogInformation("Generating invoice for order with InvoiceNumber: {InvoiceNumber}", order.InvoiceNumber);

            using var activity = activitySource.StartActivity("Generate Invoice");
            try
            {
                if (activity != null)
                {
                    activity.SetTag("order.id", order.InvoiceNumber);
                    activity.SetTag("messaging.system", "rabbitmq");
                    activity.SetTag("messaging.operation", "process");
                }

                using (var scope = serviceProvider.CreateScope())
                {
                    var saleOrderDataService = scope.ServiceProvider.GetRequiredService<ISaleOrderDataServiceClient>();
                    var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerDataService>();
                    var productRepository = scope.ServiceProvider.GetRequiredService<IProductDataAPIService>();
                    var pdfGeneratorService = scope.ServiceProvider.GetRequiredService<IInvoicePdfGeneratorService>();
                    var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceServiceClient>();

                    var saleOrderDTO = await saleOrderDataService.GetSaleOrderbyInvoiceNumber(order.InvoiceNumber);
                    var customer = await customerRepository.GetCustomerById(order.CustomerId);

                    if (saleOrderDTO.Status == OrderStatus.Cancelled || saleOrderDTO.Status == OrderStatus.Shipped || saleOrderDTO.Status == OrderStatus.Delivered)
                    {
                        string status = saleOrderDTO.Status.ToString();
                        await pdfGeneratorService.SendOrderStatusUpdateEmail(customer.Email, status, order.InvoiceNumber);
                        _logger.LogInformation("Status update email sent for order with InvoiceNumber: {InvoiceNumber}, Status: {Status}", order.InvoiceNumber, status);
                    }
                    else
                    {
                        List<InvoiceProduct> invoiceProducts = new List<InvoiceProduct>();

                        foreach (var processedProduct in order.Products)
                        {
                            var product = await productRepository.GetProductbyID(processedProduct.ProductId);
                            var invoiceProduct = new InvoiceProduct
                            {
                                ProductName = product.ProductName,
                                productColor = product.productColor,
                                productSize = product.productSize,
                                Price = product.Price ?? 0m,
                                Quantity = processedProduct.Quantity
                            };
                            invoiceProducts.Add(invoiceProduct);
                        }

                        var invoice = new Invoice
                        {
                            InvoiceNumber = saleOrderDTO.InvoiceNumber,
                            CustomerId = customer.CustomerId,
                            CustomerName = customer.CustomerName,
                            CustomerEmail = customer.Email,
                            NetTotal = saleOrderDTO.NetTotal,
                            ProductIDs = saleOrderDTO.ProductIDs,
                            Tax = saleOrderDTO.Tax ?? 0m,
                            GrandTotal = saleOrderDTO.NetTotal,
                            DeliveryAddress = saleOrderDTO.DeliveryAddress,
                            Products = invoiceProducts
                        };

                        await invoiceService.CreateInvoiceAsync(invoice);
                        await pdfGeneratorService.GenerateInvoicePdf(invoice);

                        _logger.LogInformation("Invoice generated successfully for order with InvoiceNumber: {InvoiceNumber}", order.InvoiceNumber);
                    }
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for order with InvoiceNumber: {InvoiceNumber}", order.InvoiceNumber);
            }
        }
    }
}
