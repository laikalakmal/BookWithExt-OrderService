using Microsoft.EntityFrameworkCore;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Persistance.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task SaveOrderAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.PurchaseResponse)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllOrdersAsync(int page, int pageSize)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.createdAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);
            
            _context.Orders.Update(order);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }
    }
}
