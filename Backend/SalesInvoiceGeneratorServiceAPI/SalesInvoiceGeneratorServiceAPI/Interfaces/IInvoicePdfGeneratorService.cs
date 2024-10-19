using SalesInvoiceGeneratorServiceAPI.Entities;

namespace SalesInvoiceGeneratorServiceAPI.Interfaces
{
    public interface IInvoicePdfGeneratorService
    {
      Task GenerateInvoicePdf(Invoice invoice);

    }
}
