using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResult> PurchaseProductAsync(Guid productId, int quantity);
        Task<ServiceResult> CancelPurchaseAsync(Guid productId, int quantity);
        Task<ServiceResult> CheckProductAvailabilityAsync(Guid productId);
    }
}