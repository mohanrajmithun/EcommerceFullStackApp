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

namespace SalesInvoiceGeneratorServiceAPI.services
{
    public class SaleOrderConsumer : ISaleOrderConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "ProcessedOrdersQueue";
        List<ProcessedOrder> processedOrders = new List<ProcessedOrder>();
        //private readonly IInvoicePdfGeneratorService pdfGenerator;
        //private readonly ISaleOrderDataService saleOrderDataService;
        private readonly IServiceProvider serviceProvider; // Inject the service provider
        private static readonly ActivitySource activitySource = new("SalesInvoiceGeneratorServiceAPI");







        public SaleOrderConsumer(IServiceProvider serviceProvider)
        {
            //this.pdfGenerator = pdfGenerator;
            this.serviceProvider = serviceProvider;
            //this.saleOrderDataService = saleOrderDataService;



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

        public void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

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
        }

        private void ProcessMessage(string message)
        {
            using var activity = activitySource.StartActivity("Process Sale Order");
            var processedOrder = JsonSerializer.Deserialize<ProcessedOrder>(message);
            activity?.SetTag("order.id", processedOrder?.InvoiceNumber);

            DequeSaleOrder(processedOrder);
            GenerateInvoiceSaleOrder(processedOrder);
        }

        private async void DequeSaleOrder(ProcessedOrder order)
        {
            using var activity = activitySource.StartActivity("Deque Sale Order");
            if (activity != null)
            {
                activity.SetTag("order.id", order.InvoiceNumber);
                activity.SetTag("messaging.system", "rabbitmq");
                activity.SetTag("messaging.operation", "receive");
            }
            processedOrders.Add(order);
        }


        public List<ProcessedOrder> GetProcessedSaleOrders()
        {
            return processedOrders;
        }


        private async void GenerateInvoiceSaleOrder(ProcessedOrder order)
        {
            using var activity = activitySource.StartActivity("Generate Invoice");
            if (activity != null)
            {
                activity.SetTag("order.id", order.InvoiceNumber);
                activity.SetTag("messaging.system", "rabbitmq");
                activity.SetTag("messaging.operation", "process");
            }

            using (var scope = serviceProvider.CreateScope())

            {
                var saleOrderDataService = scope.ServiceProvider.GetRequiredService<ISaleOrderDataServiceClient>();
                var CustomerRepository = scope.ServiceProvider.GetRequiredService<ICustomerDataService>();
                var ProductRepository = scope.ServiceProvider.GetRequiredService<IProductDataAPIService>();
                var pdfGeneratorService = scope.ServiceProvider.GetRequiredService<IInvoicePdfGeneratorService>();
                var InvoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceServiceClient>();

                List<InvoiceProduct> invoiceproducts = new List<InvoiceProduct>();


                SaleOrderDTO saleOrderDTO = await saleOrderDataService.GetSaleOrderbyInvoiceNumber(order.InvoiceNumber);
                Customer customer = await CustomerRepository.GetCustomerById(order.CustomerId);

                if (saleOrderDTO.Status == OrderStatus.Cancelled || saleOrderDTO.Status == OrderStatus.Shipped || saleOrderDTO.Status == OrderStatus.Delivered)
                {
                    // Send status update email without generating an invoice PDF

                    string status = saleOrderDTO.Status.ToString(); 

                    await pdfGeneratorService.SendOrderStatusUpdateEmail(customer.Email, status , order.InvoiceNumber);
                }
                else
                {

                    foreach (var ProcessedProduct in order.Products)
                    {
                        Product product = await ProductRepository.GetProductbyID(ProcessedProduct.ProductId);
                        InvoiceProduct invoiceProduct = new InvoiceProduct
                        {
                            ProductName = product.ProductName,
                            productColor = product.productColor,
                            productSize = product.productSize,
                            Price = product.Price ?? 0m,
                            Quantity = ProcessedProduct.Quantity

                        };
                        invoiceproducts.Add(invoiceProduct);


                    }

                    Invoice invoice = new Invoice
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
                        Products = invoiceproducts


                    };
                    InvoiceService.CreateInvoiceAsync(invoice);
                    pdfGeneratorService.GenerateInvoicePdf(invoice);


                }
                activity?.SetStatus(ActivityStatusCode.Ok);

            }


        }



    }
}
