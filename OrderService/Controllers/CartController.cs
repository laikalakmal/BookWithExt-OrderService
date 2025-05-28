using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using OrderService.Models;
using OrderService.Services;
using OrderService.DTO;

namespace OrderService.Controllers
{
    /// <summary>
    /// Controller for managing shopping carts
    /// </summary>
    [Route("api/carts")]
    [ApiController]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        /// <summary>
        /// Retrieves a cart by ID
        /// </summary>
        /// <param name="id">The cart ID</param>
        /// <returns>The cart details</returns>
        /// <response code="200">Returns the cart</response>
        /// <response code="404">If the cart is not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Cart), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCart(Guid id)
        {
            var cart = await _cartService.GetCartAsync(id);
            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        /// <summary>
        /// Creates a new cart
        /// </summary>
        /// <returns>The new cart ID</returns>
        /// <response code="201">Returns the new cart ID</response>
        [HttpPost]
        [ProducesResponseType(typeof(CartCreationResponse), 201)]
        public async Task<IActionResult> CreateCart()
        {
            var cartId = await _cartService.CreateCartAsync();
            var response = new CartCreationResponse { CartId = cartId };
            return CreatedAtAction(nameof(GetCart), new { id = cartId }, response);
        }

        /// <summary>
        /// Adds an item to a cart
        /// </summary>
        /// <param name="request">The item details</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Item added successfully</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost("{cartId:guid}/items")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> AddItem(Guid cartId, [FromBody] CartItemRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            var result = await _cartService.AddItemToCartAsync(cartId, request.ProductId, request.Quantity);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Updates a cart item quantity
        /// </summary>
        /// <param name="cartId">The cart ID</param>
        /// <param name="productId">The product ID</param>
        /// <param name="request">The update details</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Item updated successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the item is not found</response>
        [HttpPut("{cartId:guid}/items/{productId:guid}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateItem(Guid cartId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            if (request.Quantity < 0)
            {
                return BadRequest("Quantity cannot be negative");
            }

            var result = await _cartService.UpdateCartItemAsync(cartId, productId, request.Quantity);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Removes an item from a cart
        /// </summary>
        /// <param name="cartId">The cart ID</param>
        /// <param name="productId">The product ID to remove</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Item removed successfully</response>
        /// <response code="404">If the item is not found</response>
        [HttpDelete("{cartId:guid}/items/{productId:guid}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveItem(Guid cartId, Guid productId)
        {
            var result = await _cartService.RemoveItemFromCartAsync(cartId, productId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Checks out a cart
        /// </summary>
        /// <param name="cartId">The cart ID to checkout</param>
        /// <returns>Result of the checkout operation</returns>
        /// <response code="200">Checkout successful</response>
        /// <response code="400">If checkout fails</response>
        [HttpPost("{cartId:guid}/checkout")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> Checkout(Guid cartId)
        {
            var result = await _cartService.CheckoutCartAsync(cartId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpDelete("{cartId:guid}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCart(Guid cartId)
        {
            var result = await _cartService.DeleteCartAsync(cartId);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CartDto>), 200)]
        public async Task<IActionResult> GetAllCarts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _cartService.GetAllCartsAsync(page, pageSize);
            return Ok(result);
        }


    }

















    public class CartItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }

    public class CartCreationResponse
    {
        public Guid CartId { get; set; }
    }
}
