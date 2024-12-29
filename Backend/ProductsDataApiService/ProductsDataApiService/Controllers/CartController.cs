using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, AppDbContext appDbContext, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _appDbContext = appDbContext;
            _logger = logger;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCart(int customerId)
        {
            _logger.LogInformation("Received request to get cart for CustomerId: {CustomerId}", customerId);

            if (customerId == 0)
            {
                _logger.LogWarning("CustomerId is 0, returning empty cart.");
                return Ok(new Cart());
            }

            var cart = await _cartService.GetCartForCustomer(customerId);

            if (cart == null)
            {
                _logger.LogWarning("Cart not found for CustomerId: {CustomerId}", customerId);
                return NotFound("Cart not found.");
            }

            _logger.LogInformation("Cart retrieved successfully for CustomerId: {CustomerId}", customerId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductRequest request)
        {
            _logger.LogInformation("Received request to add ProductId: {ProductId} with Quantity: {Quantity} to CustomerId: {CustomerId}'s cart.",
                request.ProductId, request.Quantity, request.CustomerId);

            try
            {
                var cart = await _cartService.AddProductToCart(request.CustomerId, request.ProductId, request.Quantity);
                _logger.LogInformation("Product added successfully to CustomerId: {CustomerId}'s cart.", request.CustomerId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ProductId: {ProductId} to CustomerId: {CustomerId}'s cart.", request.ProductId, request.CustomerId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveProductFromCart([FromBody] RemoveProductRequest request)
        {
            _logger.LogInformation("Received request to remove ProductId: {ProductId} from CustomerId: {CustomerId}'s cart.",
                request.ProductId, request.CustomerId);

            try
            {
                var cart = await _cartService.RemoveProductFromCart(request.CustomerId, request.ProductId);
                _logger.LogInformation("Product removed successfully from CustomerId: {CustomerId}'s cart.", request.CustomerId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing ProductId: {ProductId} from CustomerId: {CustomerId}'s cart.", request.ProductId, request.CustomerId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProductQuantity([FromBody] UpdateProductQuantityRequest request)
        {
            _logger.LogInformation("Received request to update quantity of ProductId: {ProductId} to {Quantity} for CustomerId: {CustomerId}'s cart.",
                request.ProductId, request.Quantity, request.CustomerId);

            try
            {
                var cart = await _cartService.UpdateProductQuantity(request.CustomerId, request.ProductId, request.Quantity);
                _logger.LogInformation("Product quantity updated successfully for CustomerId: {CustomerId}'s cart.", request.CustomerId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity of ProductId: {ProductId} for CustomerId: {CustomerId}'s cart.", request.ProductId, request.CustomerId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            _logger.LogInformation("Received checkout request for CustomerId: {CustomerId} with DeliveryAddress: {DeliveryAddress}.",
                request.CustomerId, request.DeliveryAddress);

            var bearerToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            _logger.LogDebug("Extracted Bearer Token for checkout: {BearerToken}", bearerToken);

            try
            {
                var isCheckedOut = await _cartService.Checkout(request.CustomerId, request.DeliveryAddress, bearerToken);

                if (isCheckedOut != null)
                {
                    _logger.LogInformation("Checkout successful for CustomerId: {CustomerId}.", request.CustomerId);

                    var response = new
                    {
                        message = "Checkout successful",
                        IsCheckedOut = 1,
                        DeliveryAddress = isCheckedOut.DeliveryAddress,
                        InvoiceNumber = isCheckedOut.InvoiceNumber,
                        CustomerId = isCheckedOut.CustomerId,
                        OrderStatus = isCheckedOut.Status
                    };

                    return Ok(response);
                }

                _logger.LogWarning("Checkout failed for CustomerId: {CustomerId}.", request.CustomerId);
                return BadRequest(new
                {
                    message = "Checkout failed",
                    IsCheckedOut = 0,
                    DeliveryAddress = string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for CustomerId: {CustomerId}.", request.CustomerId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTO classes
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
