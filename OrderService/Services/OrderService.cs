using OrderService.Interfaces;
using OrderService.Models;
using OrderService.DTO;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            
            // Set creation time if not already set
            if (order.createdAt == default)
            {
                order.createdAt = DateTime.UtcNow;
            }
            
            await _orderRepository.SaveOrderAsync(order);
            return order;
        }

        public async Task<ServiceResult> DeleteOrderAsync(Guid id)
        {
            var success = await _orderRepository.DeleteOrderAsync(id);
            
            return new ServiceResult
            {
                Success = success,
                Message = success ? "Order deleted successfully." : "Order not found or could not be deleted."
            };
        }

        public async Task<OrdersListDto> GetAllOrdersAsync(int page, int pageSize)
        {
            var orders = await _orderRepository.GetAllOrdersAsync(page, pageSize);
            
            return new OrdersListDto
            {
                Orders = orders.Select(o => MapToOrderDto(o)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = orders.Count
            };
        }

        public async Task<OrderDto?> GetOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return null;
                
            return MapToOrderDto(order);
        }

        public async Task<ServiceResult> UpdateOrderStatusAsync(Guid id, string status)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Order not found."
                };
            }

            order.status = status;
            var success = await _orderRepository.UpdateOrderAsync(order);

            return new ServiceResult
            {
                Success = success,
                Message = success ? "Order status updated successfully." : "Failed to update order status."
            };
        }

        public async Task<OrderDto?> UpdateOrderAsync(Guid id, OrderDto orderDto)
        {
            var existingOrder = await _orderRepository.GetOrderByIdAsync(id);
            if (existingOrder == null)
                return null;

            // Update only allowed properties
            existingOrder.status = orderDto.Status;
            
            // Note: We don't allow updating items directly through this method
            // Items are managed through the checkout process

            var success = await _orderRepository.UpdateOrderAsync(existingOrder);
            if (!success)
                return null;

            return MapToOrderDto(existingOrder);
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CreatedAt = order.createdAt,
                Status = order.status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.productId,
                    Quantity = i.quantity,
                    PriceAtPurchase = i.priceAtPurchase,
                    PurchaseResponse = i.PurchaseResponse
                }).ToList()
            };
        }
    }
}
