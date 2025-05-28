using System;
using System.Threading.Tasks;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface ICartService
    {
        Task<Guid> CreateCartAsync();
        Task<CartDto> GetCartAsync(Guid cartId);
        Task<ServiceResult> AddItemToCartAsync(Guid cartId, Guid productId, int quantity);
        Task<ServiceResult> UpdateCartItemAsync(Guid cartId, Guid productId, int quantity);
        Task<ServiceResult> RemoveItemFromCartAsync(Guid cartId, Guid productId);
        Task<ServiceResult> CheckoutCartAsync(Guid cartId);

        Task<List<CartDto>> GetAllCartsAsync(int page, int pageSize);
        Task<ServiceResult> DeleteCartAsync(Guid cartId);
    }
}