using OrderService.DTO;
using OrderService.Interfaces;
using OrderService.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace OrderService.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public CartService(ICartRepository cartRepository, IProductService productService, IOrderService orderService)
        {
            _cartRepository = cartRepository;
            _productService = productService;
            _orderService = orderService;
        }

        public async Task<ServiceResult> AddItemToCartAsync(Guid cartId, Guid productId, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(cartId);
                if (cart == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Cart not found."
                    };
                }

                var availabilityResult = await _productService.CheckProductAvailabilityAsync(productId);
                if (!availabilityResult.Success)
                {
                    return availabilityResult;
                }

                var availability = availabilityResult.Data as AvailabilityInfo;
                if (availability == null || !availability.IsAvailable)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Product is not available."
                    };
                }

                // Check if item already exists in cart
                var existingItem = cart.Items?.FirstOrDefault(i => i.productId == productId);

                if (existingItem != null)
                {
                    // Update quantity of existing item
                    existingItem.quantity += quantity;
                    bool updateSuccessful = await _cartRepository.UpdateCartItemAsync(existingItem);

                    if (!updateSuccessful)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Failed to update cart item. The item may have been modified."
                        };
                    }
                }
                else
                {
                    // Create a new item
                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        productId = productId,
                        quantity = quantity,
                        priceAtPurchase = availability.currentPrice,
                        Cart = cart
                    };

                    // Add the cart item directly rather than updating the cart
                    bool addSuccessful = await _cartRepository.AddCartItemAsync(newItem);

                    if (!addSuccessful)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Failed to add item to cart."
                        };
                    }
                }

                return new ServiceResult
                {
                    Success = true,
                    Message = "Item added to cart successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while adding item to cart: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult> CheckoutCartAsync(Guid cartId)
        {
            // Get cart with items
            var cart = await _cartRepository.GetCartByIdAsync(cartId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return new ServiceResult{
                    Success = false,
                    Message="Cart not found or empty"
                };
            }

            // Create order items from cart items
            var orderItems = new List<OrderItem>();

            // Try to purchase each product
            foreach (var item in cart.Items)
            {
                var purchaseResult = await _productService.PurchaseProductAsync(
                    item.productId,
                    item.quantity);

                if (!purchaseResult.Success)
                {
                    // Rollback previous purchases
                 //   await RollbackPurchasesAsync(orderItems);
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Failed to purchase product {item.productId}: {purchaseResult.Message}"

                    };
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    productId = item.productId,
                    quantity = item.quantity,
                    priceAtPurchase = item.priceAtPurchase,
                    PurchaseResponse = purchaseResult.Data as PurchaseResponseDto
                };

                orderItems.Add(orderItem);
            }

            // Create and save the order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                createdAt = DateTime.UtcNow,
                Items = orderItems,
                status = "Pending"
            };

            await _orderService.CreateOrderAsync(order);

            // Delete the cart after successful checkout
            await _cartRepository.DeleteCartAsync(cartId);

            return new ServiceResult
            {
                Success = true,
                Message="Oder is confiremd",
                Data = order

            };
        }

        private async Task RollbackPurchasesAsync(List<OrderItem> successfulItems)
        {
            foreach (var item in successfulItems)
            {
                await _productService.CancelPurchaseAsync(item.productId, item.quantity);
            }
        }

        public async Task<Guid> CreateCartAsync()
        {
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                Items = []
            };

            var createdCart = await _cartRepository.CreateCartAsync(cart);
            return createdCart.Id;
        }

        public Task<ServiceResult> DeleteCartAsync(Guid cartId)
        {
            return _cartRepository.DeleteCartAsync(cartId)
                .ContinueWith(task => new ServiceResult
                {
                    Success = task.Result,
                    Message = task.Result ? "Cart deleted successfully." : "Failed to delete cart."
                });
        }

        public async Task<List<CartDto>> GetAllCartsAsync(int page, int pageSize)
        {
            var allCarts = await _cartRepository.GetAllCartsAsync();
            return allCarts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cart => ToDto(cart))
                .ToList();
        }

        public async Task<CartDto> GetCartAsync(Guid cartId)
        {
            Cart cart = await _cartRepository.GetCartByIdAsync(cartId);

            return ToDto(cart);
        }

        public async Task<ServiceResult> RemoveItemFromCartAsync(Guid cartId, Guid productId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(cartId);
                if (cart == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Cart not found."
                    };
                }

                var item = cart.Items?.FirstOrDefault(i => i.productId == productId);
                if (item == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Item not found in cart."
                    };
                }

                cart.Items?.Remove(item);
                bool updateSuccessful = await _cartRepository.UpdateCartAsync(cart);

                if (!updateSuccessful)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Failed to update cart. The cart may have been modified."
                    };
                }

                return new ServiceResult
                {
                    Success = true,
                    Message = "Item removed from cart successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while removing item from cart: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult> UpdateCartItemAsync(Guid cartId, Guid productId, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(cartId);
                if (cart == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Cart not found."
                    };
                }

                var item = cart.Items?.Find(item => item.productId == productId);
                if (item == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Item not found in cart."
                    };
                }
                bool updateSuccessful = false;
                if (quantity == 0)
                {
                    updateSuccessful = await _cartRepository.RemoveCartItemAsync(itemId: item.Id);
                }
                else
                {
                    item.quantity = quantity;
                    updateSuccessful = await _cartRepository.UpdateCartItemAsync(item);
                }


                if (!updateSuccessful)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Failed to update cart item."
                    };
                }

                return new ServiceResult
                {
                    Success = true,
                    Message = "Cart item updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while updating cart item: {ex.Message}"
                };
            }
        }


        public CartDto ToDto(Cart cart)
        {
            List<CartItemDto> items = cart.Items?.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.productId,
                Quantity = item.quantity,
                PriceAtPurchase = item.priceAtPurchase
            }).ToList() ?? [];
            return new CartDto
            {
                Id = cart.Id,
                CreatedAt = cart.CreatedAt,
                Items = items

            };
        }
    }
}
