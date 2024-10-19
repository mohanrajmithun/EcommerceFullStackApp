using SaleOrderProcessingAPI.Interfaces;

namespace SaleOrderProcessingAPI.Services
{
    public class AddressValidationService : IAddressValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<AddressValidationService> logger;

        public AddressValidationService(HttpClient httpClient, IConfiguration configuration, ILogger<AddressValidationService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Geoapify:ApiKey"];
            this.logger = logger;

        }

        public async Task<bool> IsAddressValidAsync(string address)
        {
            try
            {
                // Format the request URL
                string url = $"https://api.geoapify.com/v1/geocode/search?text={Uri.EscapeDataString(address)}&apiKey={_apiKey}";

                // Send the request
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Read the response content
                string content = await response.Content.ReadAsStringAsync();

                // Here, you can parse the content to check the status and details
                // For simplicity, assume that if we get a successful response, the address is valid
                // You might want to do more thorough checks depending on the API response structure
                return true;
            }
            catch (HttpRequestException e)
            {
                // Handle request exception
                logger.LogInformation($"Request error: {e.Message}");
                return false;
            }
        }
    }
}
