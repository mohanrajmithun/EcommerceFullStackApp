using Microsoft.EntityFrameworkCore;
using ProductsDataApiService.Entities;
using ProductsDataApiService.Infrastructure;
using ProductsDataApiService.interfaces;
using ProductsDataApiService.ServiceClients;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesOrderInvoiceAPI.Entities;

namespace ProductsDataApiService.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IProductDataAPIService _productService; // Assuming Product API interaction
        private readonly ISaleOrderDataServiceClient saleOrderServiceClient;
        private readonly ISaleOrderProcessingServiceClient saleOrderProcessingServiceClient;

        public CartService(AppDbContext context, IProductDataAPIService productService, ISaleOrderDataServiceClient saleOrderDataServiceClient, ISaleOrderProcessingServiceClient saleOrderProcessingServiceClient)
        {
            _context = context;
            _productService = productService;
            this.saleOrderServiceClient = saleOrderDataServiceClient;
            this.saleOrderProcessingServiceClient = saleOrderProcessingServiceClient;
        }

        public async Task<Cart> GetCartByCustomerId(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            return cart;
        }

        private async Task<Cart> GetOrCreateCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartDetailsInfoDTO> GetCartForCustomer(int customerId)
        {
            var cart = await _context.Carts
            .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            CartDetailsInfoDTO cartDetailsInfoDTO = new CartDetailsInfoDTO();

            if (cart == null)
            {
                Cart new_cart = await GetOrCreateCart(customerId);

                cartDetailsInfoDTO.CartId = new_cart.CartId;
                cartDetailsInfoDTO.TotalPrice = new_cart.TotalPrice;
                cartDetailsInfoDTO.DeliveryAddress = new_cart.DeliveryAddress;
                cartDetailsInfoDTO.CustomerId = customerId;

                return cartDetailsInfoDTO;


            }

            cartDetailsInfoDTO.CartId = cart.CartId;
            cartDetailsInfoDTO.TotalPrice = cart.TotalPrice;
            cartDetailsInfoDTO.DeliveryAddress = cart.DeliveryAddress;
            cartDetailsInfoDTO.CustomerId = customerId;

            foreach(var item in cart.Items)
            {
                var product = await _productService.GetProductbyID(item.ProductId);

                CartDetailsProductInfo cartDetailsProductInfo = new CartDetailsProductInfo();

                cartDetailsProductInfo.product = product;

                cartDetailsProductInfo.quantity = item.Quantity;

                cartDetailsProductInfo.subtotal = item.Subtotal;

                cartDetailsInfoDTO.Products.Add(cartDetailsProductInfo);
            }

            return cartDetailsInfoDTO;

        }

        public async Task<CartDetailsInfoDTO> AddProductToCart(int customerId, int productId, int quantity)
        {
            var cart = await GetOrCreateCart(customerId);

            var product = await _productService.GetProductbyID(productId);
            var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                if (product.StockQuantity > quantity)
                {
                    cartItem.Quantity += quantity;
                    await _productService.ReduceStockCount(productId, quantity);

                }
    
            }
            else
            {

                ProductStockInfoDTO productStockInfoDTO = await _productService.IsProductOutOfStock(product.ProductId);
                if(!productStockInfoDTO.IsOutOfStock) 
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        PricePerUnit = (decimal)product.Price
                    });
                }
                
            }

            cart.TotalPrice = cart.Items.Sum(item => item.Subtotal);
            cart.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return await GetCartForCustomer(customerId);

        }

        public async Task<CartDetailsInfoDTO> RemoveProductFromCart(int customerId, int productId)
        {
            var cart = await GetCartByCustomerId(customerId);

            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    await _productService.IncreaseStockCount(cartItem.ProductId,cartItem.Quantity);
                    cart.Items.Remove(cartItem);
                    cart.TotalPrice = cart.Items.Sum(item => item.Subtotal);
                    cart.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
            }

            return await GetCartForCustomer(customerId);
        }

        public async Task<CartDetailsInfoDTO> UpdateProductQuantity(int customerId, int productId, int quantity)
        {
            var cart = await GetCartByCustomerId(customerId);
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    cart.TotalPrice = cart.Items.Sum(item => item.Subtotal);
                    cart.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
            }

            return await GetCartForCustomer(customerId);
        }


        public async Task<SaleOrder> Checkout(int customerId, string deliveryAddress, string bearerToken)
        {
            var cart = await GetCartByCustomerId(customerId);
            if (cart != null)
            {
                cart.DeliveryAddress = deliveryAddress;
                cart.UpdatedDate = DateTime.Now;
                cart.TotalPrice = cart.Items.Sum(item => item.Subtotal);

                SaleOrder IsOrderCreated = await CreateandProcessSaleorder(cart, customerId, deliveryAddress, bearerToken, cart.TotalPrice);

                if (IsOrderCreated.Status == SalesAPILibrary.Shared_Enums.OrderStatus.Created)
                {

                    _context.Carts.Remove(cart);
                    await _context.SaveChangesAsync();
                    return IsOrderCreated;

                }


                

            }

            return null;

            
        }

        private async Task<List<int>> GetProductIDsforCart(int customerId)
        {
            var cart = await GetCartByCustomerId(customerId);
            List<int> productIds = new List<int>();

            foreach (var item in cart.Items)
            {
                productIds.Add(item.ProductId);
                
            }

            return productIds;


        }

        private async Task<(List<int> ProductIds, List<int> Quantities)> GetProductIDsAndQuantitiesForCart(int customerId)
        {
            var cart = await GetCartByCustomerId(customerId);
            List<int> productIds = new List<int>();
            List<int> quantities = new List<int>();

            foreach (var item in cart.Items)
            {
                productIds.Add(item.ProductId);
                quantities.Add(item.Quantity); // Assuming 'Quantity' exists in the CartItem
            }

            return (productIds, quantities);
        }


        private async Task<OutOfStockResponse> SendOutOfStockResponse(int ProductId)
        {
            var product = await _productService.GetProductbyID(ProductId);

            OutOfStockResponse outOfStockResponse = new OutOfStockResponse();
            outOfStockResponse.ProductName = product.ProductName;
            outOfStockResponse.message = $"{outOfStockResponse.ProductName} is out of stock now. We will notify you when it's available again.";

            return outOfStockResponse;
        }

        private async Task<SaleOrder> CreateandProcessSaleorder(Cart cart, int customerid, string DeliveryAddress , string bearertoken, decimal orderTotal)

        {

            var (productIds, quantities) = await GetProductIDsAndQuantitiesForCart(customerid);


            SaleOrderDTO saleOrderDTO = new SaleOrderDTO();

            saleOrderDTO.CustomerId = customerid;
            saleOrderDTO.DeliveryAddress = DeliveryAddress;
            saleOrderDTO.ProductIDs = productIds;
            saleOrderDTO.Quantities = quantities;
            saleOrderDTO.NetTotal = orderTotal;

            SaleOrder saleorder = await saleOrderServiceClient.CreateSaleOrder(saleOrderDTO, bearertoken);

            if(saleorder.Status == SalesAPILibrary.Shared_Enums.OrderStatus.Created) {

                List<ProcessedOrder> processedOrders = await saleOrderProcessingServiceClient.ProcessSaleOrdersAsync();

                if (processedOrders.Count > 0)
                {

                    return saleorder;
                }
            }
            return new SaleOrder();

        }

    }
}
