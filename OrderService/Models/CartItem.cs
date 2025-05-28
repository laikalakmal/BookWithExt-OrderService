namespace OrderService.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid productId { get; set; }
        public int quantity { get; set; }
        public decimal priceAtPurchase { get; set; }

        required public Cart Cart { get; set; }

        public CartItem(Cart cart)
        {
            Cart = cart;
        }

        public CartItem()
        {
            
        }
    }
}
