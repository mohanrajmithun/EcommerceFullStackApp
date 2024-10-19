using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text.Json;

namespace SaleOrderDataService.ServiceClients
{
    public class CustomerDataServiceClient: ICustomerDataServiceClient
    {
        private readonly HttpClient _httpClient;

        public CustomerDataServiceClient(HttpClient httpClient)
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
        public async Task<Customer> GetCustomerById(int customerId, string bearertoken)
        {
            AddAuthorizationHeader(bearertoken);
            var response = await _httpClient.GetAsync($"api/CustomerData/customer/?id={customerId}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Customer customer = JsonSerializer.Deserialize<Customer>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return customer;
        }
    }
}
