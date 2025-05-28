namespace OrderService.Services
{
    public class AvailabilityInfo
    {
        public string? Status { get; set; }
        public int remainingSlots { get; set; }

        public decimal currentPrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}