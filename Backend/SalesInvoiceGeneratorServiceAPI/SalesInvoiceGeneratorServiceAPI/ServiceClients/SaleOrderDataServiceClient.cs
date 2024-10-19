using ISaleOrderDataServiceClient = SalesInvoiceGeneratorServiceAPI.Interfaces.ISaleOrderDataServiceClient;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using System.Text.Json;

namespace SalesInvoiceGeneratorServiceAPI.ServiceClients
{
    public class SaleOrderDataServiceClient : ISaleOrderDataServiceClient
    {
        private readonly HttpClient _httpClient;

        public SaleOrderDataServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<SaleOrderDTO>> GetAllSaleOrders()
        {
            var response = await _httpClient.GetAsync($"api/SaleOrderDataService/GetAllSaleOrder");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            IList<SaleOrderDTO> saleOrders = JsonSerializer.Deserialize<IList<SaleOrderDTO>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return saleOrders;
        }


        public async Task<SaleOrderDTO> UpdateOrderStatusAsync(string invoiceNumber, OrderStatus orderStatus)
        {
            var response = await _httpClient.GetAsync($"api/SaleOrderDataService/UpdateOrderStatus/?invoiceNumber={invoiceNumber}&orderStatus={orderStatus}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            SaleOrderDTO saleOrderDTO = JsonSerializer.Deserialize<SaleOrderDTO>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return saleOrderDTO;

        }


        public async Task<SaleOrderDTO> GetSaleOrderbyInvoiceNumber(string invoiceNumber)
        {
            var response = await _httpClient.GetAsync($"api/SaleOrderDataService/GetSaleOrder?invoiceNumber={invoiceNumber}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            SaleOrderDTO saleOrderDTO = JsonSerializer.Deserialize<SaleOrderDTO>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return saleOrderDTO;

        }
    }
}
