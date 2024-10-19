using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsDataApiService.Infrastructure;
using ProductsDataApiService.Services;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;

namespace ProductsDataApiService.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDataAPIController: ControllerBase
    {
        private readonly IProductDataAPIService ProductDataAPIService;
        public ProductDataAPIController(IProductDataAPIService ProductDataAPIService)
        {

            this.ProductDataAPIService = ProductDataAPIService;


        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("Products")]

        public async Task<ActionResult<IList<Product>>> GetAllProducts()
        {
            try
            {
                var Products = await ProductDataAPIService.GetAllProducts();
                if (Products != null)
                {
                    return Ok(Products);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }
        [HttpGet("Product")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            try
            {
                // Validate the input parameter
                if (id < 1)
                {
                    return BadRequest("Product Id cannot be empty");
                }

                var product = await ProductDataAPIService.GetProductbyID(id);
                if (product == null)
                {
                    return NotFound($"Product with Id '{id}' not found");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }

        }

        [HttpGet("GetProductDetails")]
        public async Task<ActionResult<ProductDetails>> GetProductDetails(int id)
        {
            try
            {
                var Product = await ProductDataAPIService.GetProductDetails(id);
                if (Product != null)
                {
                    return Ok(Product);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }

        }


        [HttpPost("AddProduct")]
        public async Task<ActionResult<ProductDetails>> CreateProduct(ProductDetails ProductDetails)
        {
            var created_Product = await ProductDataAPIService.AddProduct(ProductDetails);
            return CreatedAtAction(nameof(ProductDataAPIService.GetProductbyID), new { id = created_Product.ProductId }, created_Product);
        }


        [HttpPut("updateProduct")]
        public async Task<ActionResult<ProductDetails>> UpdateProduct(ProductDetails ProductDetails)
        {
            try
            {
                if (ProductDetails == null)
                {
                    return BadRequest("Product object cannot be null");
                }

                var Product = await ProductDataAPIService.UpdateProductDetails(ProductDetails);

                if (Product == null)
                {
                    return NotFound($"the movie with id:{ProductDetails.ProductId} does not exist");
                }
                return Ok(Product);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpDelete]

        public async Task<ActionResult<ProductDetails>> DeleteProduct(int id)
        {
            try
            {
                var Productbyid = await ProductDataAPIService.GetProductbyID(id);
                if (Productbyid == null)
                {
                    return BadRequest("Product object cannot be null");
                }

                var Product = await ProductDataAPIService.DeleteProduct(id);

                if (Product == null)
                {
                    return NotFound($"the movie with id:{id} does not exist");
                }
                return Ok(Product);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpPut("UpdateStockQuantity")]

        public async Task<ActionResult<Product>> UpdateStockQuantity(int id, int count)
        {
            try
            {
                var Productbyid = await ProductDataAPIService.GetProductbyID(id);
                if (Productbyid == null)
                {
                    return BadRequest("Product object cannot be null");
                }

                var product = await ProductDataAPIService.UpdateStockQuantity(id,count);

                if (product == null)
                {
                    return NotFound($"the movie with id:{id} does not exist");
                }
                return Ok(product);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpGet("IsProductOutOfStock")]

        public async Task<ActionResult<ProductStockInfoDTO>> IsProductOutOfStock(int id)
        {
            try
            {
                var Productbyid = await ProductDataAPIService.GetProductbyID(id);
                if (Productbyid == null)
                {
                    return NotFound($"the movie with id:{id} does not exist");
                }

                ProductStockInfoDTO ProductStockInfoDTO = await ProductDataAPIService.IsProductOutOfStock(id);

               
                
                return Ok(ProductStockInfoDTO);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpPut("ReduceStockCount")]

        public async Task<ActionResult<Product>> ReduceStockCount(int id, int count)
        {
            try
            {
                var Productbyid = await ProductDataAPIService.GetProductbyID(id);
                if (Productbyid == null)
                {
                    return BadRequest("Product object cannot be null");
                }

                var product = await ProductDataAPIService.ReduceStockCount(id, count);

                if (product == null)
                {
                    return NotFound($"the movie with id:{id} does not exist");
                }
                return Ok(product);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpGet("GetProductCategories")]
        public async Task<ActionResult<List<ProductCategory>>> GetProductCategoriesAsync()
        {
            try
            {
                var categories = await ProductDataAPIService.GetProductCategoriesAsync();
                if (categories != null)
                {
                    return Ok(categories);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [HttpPut("IncreaseStockCount")]

        public async Task<ActionResult<Product>> IncreaseStockCount(int id, int count)
        {
            try
            {
                var Productbyid = await ProductDataAPIService.GetProductbyID(id);
                if (Productbyid == null)
                {
                    return BadRequest("Product object cannot be null");
                }

                var product = await ProductDataAPIService.IncreaseStockCount(id,count);

                if (product == null)
                {
                    return NotFound($"the movie with id:{id} does not exist");
                }
                return Ok(product);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }

        [HttpGet("GetProductsByCategories")]

        public async Task<ActionResult<IList<Product>>> GetProductsByCategories(int CategoryId)
        {
            var products = await ProductDataAPIService.GetProductsByCategoriesAsync(CategoryId);

            return Ok(products);    
        }

    }
}
