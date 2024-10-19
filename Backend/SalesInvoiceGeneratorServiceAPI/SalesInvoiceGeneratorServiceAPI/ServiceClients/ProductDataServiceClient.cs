using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text.Json;

namespace SalesInvoiceGeneratorServiceAPI.ServiceClients
{
    public class ProductDataServiceClient : IProductDataServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductDataServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Product> GetProductbyID(int productId, string token)
        {
            var response = await _httpClient.GetAsync($"api/ProductDataAPI/Product/?id={productId}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Product product = JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return product;
        }
    }
}
