namespace OrderService.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid productId { get; set; }
        public int quantity { get; set; }
        public decimal priceAtPurchase { get; set; }
    }
}
