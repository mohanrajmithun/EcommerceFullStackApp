using Microsoft.AspNetCore.Mvc;
using SalesAPILibrary.Shared_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface IProductDataAPIService
    {
        Task<IList<Product>> GetAllProducts();

        Task<ProductDetails> GetProductDetails(int id);

        Task<List<Product>> GetProductsbyCodes(List<string> codes);

        Task<Product> GetProductbyID(int id);

        Task<Product> UpdateProductDetails(ProductDetails productDetails);

        Task<ProductDetails> AddProduct(ProductDetails productDetails);

        Task<ProductDetails> DeleteProduct(int id);

        Task<Product> UpdateStockQuantity(int id, int count);

        Task<ProductStockInfoDTO> IsProductOutOfStock(int id);

        Task<Product> ReduceStockCount(int id, int count);

        Task<Product> IncreaseStockCount(int id, int count);

        Task<List<ProductCategory>> GetProductCategoriesAsync();

        Task<List<Product>> GetProductsByCategoriesAsync(int categoryId);
    }
}
