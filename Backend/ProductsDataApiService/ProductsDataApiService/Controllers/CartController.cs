using Microsoft.AspNetCore.Mvc;
using ProductsDataApiService.Entities;
using ProductsDataApiService.Infrastructure;
using ProductsDataApiService.interfaces;
using SalesAPILibrary.Shared_Entities;

namespace ProductsDataApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _appDbContext;

        public CartController(ICartService cartService, AppDbContext appDbContext)
        {
            _cartService = cartService;
            _appDbContext = appDbContext;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCart(int customerId)
        {

            if (customerId == 0)
            {
                Cart empty_cart = new Cart();
                return Ok(empty_cart);
            }
            var cart = await _cartService.GetCartForCustomer(customerId);
            if (cart == null)
            {
                return NotFound("Cart not found.");
            }
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductRequest request)
        {
            var cart = await _cartService.AddProductToCart(request.CustomerId, request.ProductId, request.Quantity);
            return Ok(cart);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveProductFromCart([FromBody] RemoveProductRequest request)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var cart = await _cartService.RemoveProductFromCart(request.CustomerId, request.ProductId);
                    //await transaction.CommitAsync();

                    return Ok(cart);

                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();

                    return BadRequest(new { message = ex.Message });
                }
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProductQuantity([FromBody] UpdateProductQuantityRequest request)
        {
            var cart = await _cartService.UpdateProductQuantity(request.CustomerId, request.ProductId, request.Quantity);
            return Ok(cart);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var bearerToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            Console.WriteLine(bearerToken);

            SaleOrder isCheckedOut = await _cartService.Checkout(request.CustomerId, request.DeliveryAddress, bearerToken);

            if (isCheckedOut != null)
            {
                var response = new
                {
                    message = "Checkout successful",
                    IsCheckedOut = 1,
                    DeliveryAddress = isCheckedOut.DeliveryAddress,
                    InvoiceNumber = isCheckedOut.InvoiceNumber,
                    CustomerId = isCheckedOut.CustomerId,
                    OrderStatus = isCheckedOut.Status
                };

                return Ok(response); // Returning the anonymous object directly as a JSON result
            }

            return BadRequest(new
            {
                message = "Checkout failed",
                IsCheckedOut = 0,
                DeliveryAddress = string.Empty
            }); ;
        }
    }

    public class AddProductRequest
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveProductRequest
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
    }

    public class UpdateProductQuantityRequest
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutRequest
    {
        public int CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
    }

}