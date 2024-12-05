
using SalesAPILibrary.Shared_Entities;

namespace SalesInvoiceGeneratorServiceAPI.Interfaces
{
    public interface IInvoicePdfGeneratorService
    {
      Task GenerateInvoicePdf(Invoice invoice);

        Task SendOrderStatusUpdateEmail(string customerEmail, string orderStatus, string invoiceNumber);

    }
}
