namespace OrderService.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string status { get; set; } = "Pending";
    }
}
