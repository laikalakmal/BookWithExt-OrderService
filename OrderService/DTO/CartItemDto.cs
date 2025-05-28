namespace OrderService.DTO
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }

    }
}