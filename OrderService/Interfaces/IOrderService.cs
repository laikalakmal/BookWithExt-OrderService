using OrderService.Models;
using OrderService.DTO;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
    Task<ServiceResult> DeleteOrderAsync(Guid id);
    Task<OrdersListDto> GetAllOrdersAsync(int page, int pageSize);
    Task<OrderDto?> GetOrderAsync(Guid id);
    Task<ServiceResult> UpdateOrderStatusAsync(Guid id, string status);
    Task<OrderDto?> UpdateOrderAsync(Guid id, OrderDto orderDto);
}
