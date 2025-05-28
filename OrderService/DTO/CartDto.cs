using Microsoft.AspNetCore.Http.HttpResults;

namespace OrderService.DTO
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<CartItemDto>? Items {get;set;}

    }
}
