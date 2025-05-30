using Microsoft.EntityFrameworkCore;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Persistance.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _dbContext;

        public CartRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "Database context do not have injected");
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            await _dbContext.Carts.AddAsync(cart);
            await _dbContext.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> GetCartByIdAsync(Guid cartId)
        {
            try
            {
                Cart? cart = await _dbContext.Carts
                       .Include(c => c.Items)
                       .FirstOrDefaultAsync(c => c.Id == cartId);
                return cart?? throw new Exception("No cart for such id");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddCartItemAsync(CartItem item)
        {
            await _dbContext.CartItems.AddAsync(item);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCartItemAsync(CartItem item)
        {
            _dbContext.CartItems.Update(item);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveCartItemAsync(Guid itemId)
        {
            var item = await _dbContext.CartItems.FindAsync(itemId);
            if (item == null) return false;
            
            _dbContext.CartItems.Remove(item);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCartAsync(Cart cart)
        {
            _dbContext.Carts.Update(cart);
            return await _dbContext.SaveChangesAsync()>0;
        }

        public async Task<List<Cart>> GetAllCartsAsync()
        {
            
            return await _dbContext.Carts.ToListAsync();

        }

        public async Task<bool> DeleteCartAsync(Guid cartId)
        {
            return await _dbContext.Carts
                .Where(c => c.Id == cartId)
                .ExecuteDeleteAsync() > 0;
        }

        async Task<bool> ICartRepository.UpdateCartAsync(Cart cart)
        {
            return await UpdateCartAsync(cart);
        }
    }
}
