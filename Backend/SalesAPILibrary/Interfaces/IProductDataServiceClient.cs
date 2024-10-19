using SalesAPILibrary.Shared_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface IProductDataServiceClient
    {
        Task<Product> GetProductbyID(int productId, string bearerToken);

    }
}
