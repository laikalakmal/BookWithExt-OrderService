using OrderService.DTO;
using OrderService.Models;
using OrderService.Persistance.Repository;
using System.Reflection.Metadata.Ecma335;

namespace OrderService.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly HttpClient _httpClient;
        private readonly string _productServiceBaseUrl = "http://localhost:8080/api/Product";

        public CartService(ICartRepository cartRepository, HttpClient httpClient)
        {
            _cartRepository = cartRepository;
            _httpClient = httpClient;
        }

        public async Task<ServiceResult> AddItemToCartAsync(Guid cartId, Guid productId, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartAsync(cartId);
                if (cart == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Cart not found."
                    };
                }
                
                var response = await _httpClient.GetAsync($"{_productServiceBaseUrl}/{productId}/availability");
                if (response == null || !response.IsSuccessStatusCode)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Product not found or service unavailable"
                    };
                }

                AvailabilityInfo? availability = await response.Content.ReadFromJsonAsync<AvailabilityInfo>();

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

        public Task<ServiceResult> CheckoutCartAsync(Guid cartId)
        {
            throw new NotImplementedException();
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
            Cart cart = await _cartRepository.GetCartAsync(cartId);

            return ToDto(cart);
        }

        public async Task<ServiceResult> RemoveItemFromCartAsync(Guid cartId, Guid productId)
        {
            try
            {
                var cart = await _cartRepository.GetCartAsync(cartId);
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
                var cart = await _cartRepository.GetCartAsync(cartId);
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
                  updateSuccessful= await _cartRepository.RemoveCartItemAsync(itemId: item.Id);
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
                Items=items

            };
        }
    }
}
