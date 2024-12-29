using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using ProductsDataApiService.Infrastructure;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using System;

namespace ProductsDataApiService.Services
{
    public class ProductDataAPIService : IProductDataAPIService
    {
        private readonly ILogger<ProductDataAPIService> logger;
        private readonly AppDbContext appDbContext;

        public ProductDataAPIService(AppDbContext appDbContext, ILogger<ProductDataAPIService> logger)
        {
            this.logger = logger;
            this.appDbContext = appDbContext;
        }

        public async Task<IList<Product>> GetAllProducts()
        {
            logger.LogInformation("Fetching all products...");
            var products = await appDbContext.Products.ToListAsync();
            logger.LogInformation($"Fetched {products.Count} products.");
            return products;
        }

        public async Task<Product> GetProductbyID(int id)
        {
            logger.LogInformation($"Fetching product with ID {id}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
            }
            return product;
        }

        public async Task<List<Product>> GetProductsbyCodes(List<string> codes)
        {
            logger.LogInformation("Fetching products by codes...");
            if (codes == null || codes.Count == 0)
            {
                logger.LogWarning("No codes provided.");
                return null;
            }

            var products = new List<Product>();
            foreach (var code in codes)
            {
                var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductCode == code);
                if (product != null)
                {
                    logger.LogInformation($"Product with code {code} found.");
                    products.Add(product);
                }
                else
                {
                    logger.LogWarning($"Product with code {code} not found.");
                }
            }

            logger.LogInformation($"Fetched {products.Count} products by codes.");
            return products;
        }

        public async Task<ProductDetails> GetProductDetails(int id)
        {
            logger.LogInformation($"Fetching product details for ID {id}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
                return null;
            }

            var category = await appDbContext.FindAsync<ProductCategory>(product.CategoryId);
            logger.LogInformation($"Fetched category {category?.Name} for product ID {id}.");

            return new ProductDetails
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                productColor = product.productColor,
                productSize = product.productSize,
                Price = product.Price,
                Category = category?.Name,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<Product> UpdateProductDetails(ProductDetails productDetails)
        {
            logger.LogInformation($"Updating product details for ID {productDetails.ProductId}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == productDetails.ProductId);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {productDetails.ProductId} not found.");
                return null;
            }

            var category = await appDbContext.Set<ProductCategory>()
                                .FirstOrDefaultAsync(c => c.Name == productDetails.Category);
            logger.LogInformation($"Fetched category {category?.Name} for update.");

            product.ProductName = productDetails.ProductName;
            product.ProductCode = productDetails.ProductCode;
            product.productColor = productDetails.productColor;
            product.productSize = productDetails.productSize;
            product.Price = productDetails.Price;
            product.Category = category;
            product.StockQuantity = productDetails.StockQuantity;

            await appDbContext.SaveChangesAsync();
            logger.LogInformation($"Product with ID {productDetails.ProductId} updated successfully.");

            return product;
        }

        public async Task<ProductDetails> AddProduct(ProductDetails productDetails)
        {
            logger.LogInformation("Adding a new product...");
            var category = await appDbContext.Set<ProductCategory>()
                                .FirstOrDefaultAsync(c => c.Name == productDetails.Category);

            var product = new Product
            {
                ProductName = productDetails.ProductName,
                ProductCode = productDetails.ProductCode,
                productColor = productDetails.productColor,
                productSize = productDetails.productSize,
                Price = productDetails.Price,
                Category = category,
                StockQuantity = productDetails.StockQuantity
            };

            await appDbContext.Products.AddAsync(product);
            await appDbContext.SaveChangesAsync();
            logger.LogInformation($"New product added with ID {product.ProductId}.");

            return new ProductDetails
            {
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                productColor = product.productColor,
                productSize = product.productSize,
                Price = product.Price,
                Category = category?.Name,
                ProductId = product.ProductId,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<ProductDetails> DeleteProduct(int id)
        {
            logger.LogInformation($"Deleting product with ID {id}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
                return null;
            }

            appDbContext.Products.Remove(product);
            var result = await appDbContext.SaveChangesAsync();
            logger.LogInformation($"Product with ID {id} deleted successfully.");

            return new ProductDetails
            {
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                productColor = product.productColor,
                productSize = product.productSize,
                Price = product.Price,
                Category = (await appDbContext.FindAsync<ProductCategory>(product.CategoryId))?.Name,
                StockQuantity = product.StockQuantity
            };
        }

        public async Task<ProductStockInfoDTO> IsProductOutOfStock(int id)
        {
            logger.LogInformation($"Checking stock for product ID {id}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
                return null;
            }

            return new ProductStockInfoDTO
            {
                IsOutOfStock = product.StockQuantity <= 0,
                StockCount = product.StockQuantity
            };
        }

        public async Task<Product> UpdateStockQuantity(int id, int count)
        {
            logger.LogInformation($"Updating stock quantity for product ID {id} to {count}...");
            var product = await appDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                logger.LogWarning($"Product with ID {id} not found.");
                return null;
            }

            product.StockQuantity = count;
            await appDbContext.SaveChangesAsync();
            logger.LogInformation($"Stock quantity for product ID {id} updated to {count}.");

            return product;
        }

        public async Task<Product> ReduceStockCount(int id, int count)
        {
            logger.LogInformation($"updating product details with id {id}...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(prod => prod.ProductId == id);




            if (product != null)
            {


                product.StockQuantity = Math.Max(0, product.StockQuantity - count);

                await appDbContext.SaveChangesAsync();

                return product;
            }

            return null;

        }

        public async Task<Product> IncreaseStockCount(int id, int count)
        {
            logger.LogInformation($"updating product details with id {id}...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(prod => prod.ProductId == id);




            if (product != null)
            {

                product.StockQuantity = Math.Min(100, product.StockQuantity + count);

                await appDbContext.SaveChangesAsync();

                return product;
            }

            return null;

        }

        public async Task<List<ProductCategory>> GetProductCategoriesAsync()
        {
            logger.LogInformation("Fetching all product categories...");
            var categories = await appDbContext.ProductCategory.ToListAsync();
            logger.LogInformation($"Fetched {categories.Count} product categories.");
            return categories;
        }

        public async Task<List<Product>> GetProductsByCategoriesAsync(int categoryId)
        {
            logger.LogInformation($"Fetching products for category ID {categoryId}...");
            var products = await appDbContext.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
            logger.LogInformation($"Fetched {products.Count} products for category ID {categoryId}.");
            return products;
        }
    }
}
