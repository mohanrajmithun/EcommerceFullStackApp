using Newtonsoft.Json;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text;
using System.Text.Json;

namespace SalesInvoiceGeneratorServiceAPI.ServiceClients
{
    public class InvoiceServiceClient: IInvoiceServiceClient
    {
        private readonly HttpClient _httpClient;

        public InvoiceServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            var content = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/invoices", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var createdInvoice = JsonConvert.DeserializeObject<Invoice>(responseData);
                return createdInvoice;
            }

            // Handle errors (for example, log the error or throw an exception)
            throw new Exception("Failed to create invoice");
        }

        public async Task<Invoice> GetInvoice(string invoiceNumber)
        {
            var response = await _httpClient.GetAsync($"api/invoices/{invoiceNumber}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Invoice invoice = System.Text.Json.JsonSerializer.Deserialize<Invoice>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return invoice;

        }
    }
}
