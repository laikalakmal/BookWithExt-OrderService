using Microsoft.AspNetCore.Mvc;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    /// <summary>
    /// Controller for managing orders
    /// </summary>
    [Route("api/orders")]
    [ApiController]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <returns>The order details</returns>
        /// <response code="200">Returns the order</response>
        /// <response code="404">If the order is not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        /// <summary>
        /// Gets all orders with pagination
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of orders</returns>
        /// <response code="200">Returns the list of orders</response>
        [HttpGet]
        [ProducesResponseType(typeof(OrdersListDto), 200)]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(orders);
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="orderDto">The order to create</param>
        /// <returns>The created order</returns>
        /// <response code="201">Returns the newly created order</response>
        /// <response code="400">If the order is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
                return BadRequest("Order cannot be null");

            try
            {
                // Convert DTO to domain model
                var order = new Order
                {
                    Id = orderDto.Id != Guid.Empty ? orderDto.Id : Guid.NewGuid(),
                    createdAt = DateTime.UtcNow,
                    status = orderDto.Status,
                    Items = orderDto.Items.Select(i => new OrderItem
                    {
                        Id = i.Id != Guid.Empty ? i.Id : Guid.NewGuid(),
                        productId = i.ProductId,
                        quantity = i.Quantity,
                        priceAtPurchase = i.PriceAtPurchase,
                        PurchaseResponse = i.PurchaseResponse
                    }).ToList()
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);
                
                // Convert back to DTO for response
                var createdOrderDto = new OrderDto
                {
                    Id = createdOrder.Id,
                    CreatedAt = createdOrder.createdAt,
                    Status = createdOrder.status,
                    Items = createdOrder.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.productId,
                        Quantity = i.quantity,
                        PriceAtPurchase = i.priceAtPurchase,
                        PurchaseResponse = i.PurchaseResponse
                    }).ToList()
                };
                
                return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrderDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing order
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <param name="orderDto">The updated order data</param>
        /// <returns>The updated order</returns>
        /// <response code="200">Returns the updated order</response>
        /// <response code="400">If the order is invalid</response>
        /// <response code="404">If the order is not found</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderDto orderDto)
        {
            if (orderDto == null || id != orderDto.Id)
                return BadRequest("Invalid order data");

            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDto);
                if (updatedOrder == null)
                    return NotFound("Order not found");

                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="id">The order ID to delete</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Order deleted successfully</response>
        /// <response code="404">If the order is not found</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var result = await _orderService.DeleteOrderAsync(id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Updates an order's status
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <param name="request">The update status request</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the order is not found</response>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ServiceResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest("Status cannot be empty");
            }

            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
