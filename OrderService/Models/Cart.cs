namespace OrderService.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public List<CartItem>? Items { get; set; } 

        public DateTime CreatedAt { get; set; }

        public Cart()
        {
            CreatedAt = DateTime.UtcNow;
        }


    }
}
