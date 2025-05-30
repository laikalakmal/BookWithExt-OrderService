using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> CreateCartAsync(Cart cart);
        Task<Cart> GetCartByIdAsync(Guid cartId);
        Task<List<Cart>> GetAllCartsAsync();
        Task<bool> DeleteCartAsync(Guid cartId);
        Task<bool> UpdateCartItemAsync(CartItem item);
        Task<bool> RemoveCartItemAsync(Guid itemId);
        Task<bool> UpdateCartAsync(Cart cart);
        Task<bool> AddCartItemAsync(CartItem newItem);
    }
}