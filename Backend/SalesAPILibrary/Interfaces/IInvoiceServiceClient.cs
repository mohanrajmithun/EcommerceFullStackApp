using Microsoft.AspNetCore.Mvc;
using SalesAPILibrary.Shared_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface IInvoiceServiceClient
    {
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);

        Task<Invoice> GetInvoice(string InvoiceNumber);

    }
}
