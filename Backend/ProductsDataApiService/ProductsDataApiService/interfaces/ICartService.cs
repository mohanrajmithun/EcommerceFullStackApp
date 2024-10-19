using ProductsDataApiService.Entities;
using SalesAPILibrary.Shared_Entities;

namespace ProductsDataApiService.interfaces
{
    public interface ICartService
    {
        Task<Cart> GetCartByCustomerId(int customerId);

        Task<CartDetailsInfoDTO> AddProductToCart(int customerId, int productId, int quantity);

        //Task<Cart> GetOrCreateCart(int customerId);

        Task<CartDetailsInfoDTO> GetCartForCustomer(int customerId);

        Task<SaleOrder> Checkout(int customerId, string deliveryAddress, string bearerToken);


        Task<CartDetailsInfoDTO> UpdateProductQuantity(int customerId, int productId, int quantity);

        Task<CartDetailsInfoDTO> RemoveProductFromCart(int customerId, int productId);

    }
}
