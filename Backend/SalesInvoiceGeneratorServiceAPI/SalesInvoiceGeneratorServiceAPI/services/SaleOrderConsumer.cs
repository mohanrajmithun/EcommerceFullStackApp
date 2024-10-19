using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using SalesOrderInvoiceAPI.Entities;
using SalesInvoiceGeneratorServiceAPI.Entities;
using System.Text;
using System.Text.Json;
using InvoiceProduct = SalesInvoiceGeneratorServiceAPI.Entities.InvoiceProduct;
using Invoice = SalesInvoiceGeneratorServiceAPI.Entities.Invoice;
using SalesInvoiceGeneratorServiceAPI.ServiceClients;
using ISaleOrderDataServiceClient = SalesInvoiceGeneratorServiceAPI.Interfaces.ISaleOrderDataServiceClient;

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
                ProcessMessage(message);
            };
            _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
        }

        private void ProcessMessage(string message)
        {
            var processedOrder = JsonSerializer.Deserialize<ProcessedOrder>(message);
            DequeSaleOrder(processedOrder);
            GenerateInvoiceSaleOrder(processedOrder);
        }

        private async void DequeSaleOrder(ProcessedOrder order)
        {

            processedOrders.Add(order);
        }


        public List<ProcessedOrder> GetProcessedSaleOrders()
        {
            return processedOrders;
        }


        private async void GenerateInvoiceSaleOrder(ProcessedOrder order)
        {
            using (var scope = serviceProvider.CreateScope())

            {
                var saleOrderDataService = scope.ServiceProvider.GetRequiredService<ISaleOrderDataServiceClient>();
                var CustomerRepository = scope.ServiceProvider.GetRequiredService<ICustomerDataService>();
                var ProductRepository = scope.ServiceProvider.GetRequiredService<IProductDataAPIService>();
                List<InvoiceProduct> invoiceproducts = new List<InvoiceProduct>();


                SaleOrderDTO saleOrderDTO = await saleOrderDataService.GetSaleOrderbyInvoiceNumber(order.InvoiceNumber);
                Customer customer = await CustomerRepository.GetCustomerById(order.CustomerId);

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
                    CustomerName = customer.CustomerName,
                    CustomerEmail = customer.Email,
                    NetTotal = saleOrderDTO.NetTotal,
                    ProductIDs = saleOrderDTO.ProductIDs,
                    Tax = saleOrderDTO.Tax ?? 0m,
                    GrandTotal = saleOrderDTO.NetTotal + saleOrderDTO.Tax ?? 0m,
                    DeliveryAddress = saleOrderDTO.DeliveryAddress,
                    Products = invoiceproducts


                };
                var pdfGeneratorService = scope.ServiceProvider.GetRequiredService<IInvoicePdfGeneratorService>();
                pdfGeneratorService.GenerateInvoicePdf(invoice);

            }

        }



    }
}
