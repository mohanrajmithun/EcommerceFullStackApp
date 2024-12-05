using SalesAPILibrary.Shared_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface IInvoiceService
    {
        Task<List<Invoice>> GetAllInvoicesAsync();
        Task<Invoice> GetInvoiceByNumberAsync(string invoiceNumber);
        Task CreateInvoiceAsync(Invoice invoice);

        Task<List<Invoice>> GetInvoicesForCustomerAsync(int customerId);

        string GenerateInvoicePdf(Invoice invoice);

    }
}
