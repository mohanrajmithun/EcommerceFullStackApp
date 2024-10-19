using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text.Json;

namespace SalesInvoiceGeneratorServiceAPI.ServiceClients
{
    public class CustomerDataServiceClient: ICustomerDataServiceClient
    {
        private readonly HttpClient _httpClient;

        public CustomerDataServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Customer> GetCustomerById(int customerId,string token)
        {
            var response = await _httpClient.GetAsync($"api/CustomerData/customer/?id={customerId}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Customer customer = JsonSerializer.Deserialize<Customer>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return customer;
        }
    }
}
