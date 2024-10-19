using Microsoft.AspNetCore.Mvc;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using System.Text;
using System.Text.Json;

namespace ProductsDataApiService.ServiceClients
{
    public class SaleOrderDataServiceClient : ISaleOrderDataServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SaleOrderDataServiceClient> logger;

        public SaleOrderDataServiceClient(HttpClient httpClient, ILogger<SaleOrderDataServiceClient> logger)
        {
            _httpClient = httpClient;
            this.logger = logger;
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
        private void AddAuthorizationHeader(string bearerToken)
        {
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            }
        }

        public async Task<SaleOrder> CreateSaleOrder(SaleOrderDTO saleOrder, string bearertoken)
        {
            AddAuthorizationHeader(bearertoken);
            var content = new StringContent(JsonSerializer.Serialize(saleOrder), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/SaleOrderDataService/CreateSaleOrder", content);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"Created a sale order: {saleOrder}");
            string responseBody = await response.Content.ReadAsStringAsync();
            SaleOrder newSaleOrder = JsonSerializer.Deserialize<SaleOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return newSaleOrder;


        }

    }
}
