using SalesAPILibrary.Interfaces;
using SalesOrderInvoiceAPI.Entities;
using System.Text.Json;

namespace ProductsDataApiService.ServiceClients
{
    public class SaleOrderProcessingServiceClient : ISaleOrderProcessingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SaleOrderProcessingServiceClient> _logger;
        private readonly string _baseUrl = "https://localhost:7101/api/SaleOrderProcessing"; // Update with your actual base URL

        public SaleOrderProcessingServiceClient(HttpClient httpClient, ILogger<SaleOrderProcessingServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProcessedOrder>> ProcessSaleOrdersAsync()
        {
            try
            {
                // Log the request information
                _logger.LogInformation("Sending request to process sale orders...");

                // Send the GET request to the API endpoint
                HttpResponseMessage response = await _httpClient.GetAsync($"api/SaleOrderProcessing/ProcessSaleOrders");

                // Ensure the response was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON into a list of ProcessedOrder objects
                List<ProcessedOrder> processedOrders = JsonSerializer.Deserialize<List<ProcessedOrder>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Successfully processed sale orders.");

                return processedOrders;
            }
            catch (HttpRequestException ex)
            {
                // Log the error details
                _logger.LogError(ex, "Error occurred while processing sale orders");
                throw;
            }
            catch (Exception ex)
            {
                // Log general errors
                _logger.LogError(ex, "An unexpected error occurred");
                throw;
            }
        }
    }
}
