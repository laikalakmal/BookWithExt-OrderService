using OrderService.Models;

namespace OrderService.Persistance.Repository
{
    public interface ICartRepository
    {
        Task<Cart> CreateCartAsync(Cart cart);
        Task<Cart> GetCartAsync(Guid cartId);
        Task<List<Cart>> GetAllCartsAsync();
        Task<bool> DeleteCartAsync(Guid cartId);
        Task<bool> UpdateCartItemAsync(CartItem item);
        Task<bool> RemoveCartItemAsync(Guid itemId);
        Task<bool> UpdateCartAsync(Cart cart);
        Task<bool> AddCartItemAsync(CartItem newItem);
    }
}