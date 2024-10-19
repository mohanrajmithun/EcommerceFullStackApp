using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaleOrderDataService.ServiceClients
{
    public class ProductDataServiceClient: IProductDataServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProductDataServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        private void AddAuthorizationHeader(string bearerToken)
        {
            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            }
        }
        public async Task<Product> GetProductbyID(int productId, string bearerToken)
        {
            AddAuthorizationHeader(bearerToken);
            var response = await _httpClient.GetAsync($"api/ProductDataAPI/Product/?id={productId}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Body: " + responseBody); // For debugging purposes

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter()); // Add enum string converter

                Product product = JsonSerializer.Deserialize<Product>(responseBody, options);

                if (product == null)
                {
                    throw new Exception("Product deserialization failed or product not found.");
                }

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                throw;
            }

           
        }
    }
}
