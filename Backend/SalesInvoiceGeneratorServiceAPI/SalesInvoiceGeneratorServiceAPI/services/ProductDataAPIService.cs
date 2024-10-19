using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesInvoiceGeneratorServiceAPI.Infrastructure;

namespace SalesInvoiceGeneratorServiceAPI.services
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
            logger.LogInformation("Fetching all Products ...");

            IList<Product> Products = await appDbContext.Products.ToListAsync();



            return Products;

        }

        public async Task<Product> GetProductbyID(int id)
        {
            logger.LogInformation($"Fetching Product with ID {id} ...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(product => product.ProductId == id);


            return product;



        }


        public async Task<List<Product>> GetProductsbyCodes(List<string> codes)
        {
            if (codes != null)
            {
                List<Product> products = new List<Product>();

                foreach (string code in codes)
                {
                    Product product = await appDbContext.Products.SingleOrDefaultAsync(product => product.ProductCode == code);

                    if (product != null)
                    {
                        products.Add(product);
                    }


                }

                return products;

            }

            return null;
        }

        public async Task<ProductDetails> GetProductDetails(int id)
        {
            logger.LogInformation("Fetching Product details ...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(product => product.ProductId == id);

            if (product != null)
            {
                ProductCategory productCategory = await appDbContext.FindAsync<ProductCategory>(product.CategoryId);
                ProductDetails productDetails = new ProductDetails
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductCode = product.ProductCode,
                    productColor = product.productColor,
                    productSize = product.productSize,
                    Price = product.Price,
                    Category = productCategory.Name,
                    StockQuantity = product.StockQuantity
                };

                return productDetails;


            }

            return null;





        }

        public async Task<Product> UpdateProductDetails(ProductDetails productDetails)
        {
            logger.LogInformation($"updating product details with id {productDetails.ProductId}...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(prod => prod.ProductId == productDetails.ProductId);

            ProductCategory productCategory = await appDbContext.Set<ProductCategory>()
                                                .FirstOrDefaultAsync(pc => pc.Name == productDetails.Category);



            if (product != null)
            {
                product.ProductName = productDetails.ProductName;
                product.ProductCode = productDetails.ProductCode;
                product.productColor = productDetails.productColor;
                product.productSize = productDetails.productSize;
                product.Price = productDetails.Price;
                product.Category = productCategory;
                product.StockQuantity = productDetails.StockQuantity;

                await appDbContext.SaveChangesAsync();
                return product;
            }

            return null;

        }


        public async Task<ProductDetails> AddProduct(ProductDetails productDetails)
        {
            logger.LogInformation("Creating a new product");
            ProductCategory productCategory = await appDbContext.Set<ProductCategory>()
                                                .FirstOrDefaultAsync(pc => pc.Name == productDetails.Category);

            Product product = new Product()
            {
                ProductName = productDetails.ProductName,
                ProductCode = productDetails.ProductCode,
                productColor = productDetails.productColor,
                productSize = productDetails.productSize,
                Price = productDetails.Price,
                Category = productCategory,
                StockQuantity = productDetails.StockQuantity


            };

            var result = await appDbContext.Products.AddAsync(product);

            await appDbContext.SaveChangesAsync();


            ProductDetails productDetails1 = new ProductDetails()
            {
                ProductName = result.Entity.ProductName,
                ProductCode = result.Entity.ProductCode,
                productColor = result.Entity.productColor,
                productSize = result.Entity.productSize,
                Price = result.Entity.Price,
                Category = result.Entity.Category.Name,
                ProductId = result.Entity.ProductId,
                StockQuantity = result.Entity.StockQuantity

            };

            return productDetails1;
        }

        public async Task<ProductDetails> DeleteProduct(int id)
        {
            logger.LogInformation("Deleting a Product");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(product => product.ProductId == id);
            ProductCategory productCategory = await appDbContext.FindAsync<ProductCategory>(product.CategoryId);


            ProductDetails productDetails = new ProductDetails()
            {
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                productColor = product.productColor,
                productSize = product.productSize,
                Price = product.Price,
                Category = productCategory.Name,
                StockQuantity = product.StockQuantity

            };

            appDbContext.Products.Remove(product);
            var result = await appDbContext.SaveChangesAsync();

            if (result > 0)
            {
                return productDetails;
            }
            return null;


        }

        public async Task<ProductStockInfoDTO> IsProductOutOfStock(int id)
        {
            Product product = await appDbContext.Products.SingleOrDefaultAsync(prod => prod.ProductId == id);

            ProductStockInfoDTO productStockInfoDTO = new ProductStockInfoDTO();


            if (product != null)
            {
                if (product.StockQuantity > 0)
                {
                    productStockInfoDTO.IsOutOfStock = false;

                    productStockInfoDTO.StockCount = product.StockQuantity;

                }

                else
                {
                    productStockInfoDTO.IsOutOfStock = true;

                    productStockInfoDTO.StockCount = product.StockQuantity;
                }

            }
            return productStockInfoDTO;
        }

        public async Task<Product> UpdateStockQuantity(int id, int count)
        {
            logger.LogInformation($"updating product details with id {id}...");

            Product product = await appDbContext.Products.SingleOrDefaultAsync(prod => prod.ProductId == id);




            if (product != null)
            {

                product.StockQuantity = count;

                await appDbContext.SaveChangesAsync();

                return product;
            }

            return null;

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
            List<ProductCategory> categories = await appDbContext.ProductCategory
                .FromSqlRaw("SELECT * FROM ProductCategory")
        .ToListAsync(); // Use ToListAsync() for async queries

            return categories;
        }


        public async Task<List<Product>> GetProductsByCategoriesAsync(int categoryId)
        {
            List<Product> products = await appDbContext.Products.Where(p => p.CategoryId == categoryId) // Filter by category ID
                      .ToListAsync(); // Execute query asynchronously and convert to a list

            return products;
        }




    }

}
