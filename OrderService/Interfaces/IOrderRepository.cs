using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IOrderRepository
    {
        Task SaveOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<List<Order>> GetAllOrdersAsync(int page, int pageSize);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(Guid id);
    }
}
