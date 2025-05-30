namespace OrderService.DTO
{
    public class PurchaseResponseDto
    {
        public string? TransactionId { get; set; }

        public PurchaseResponseDto(string transactionId, string externalId)
        {
            TransactionId = transactionId;
            ExternalId = externalId;
        }

        public PurchaseResponseDto(string externalId)
        {
            ExternalId = externalId;
        }

        public PurchaseResponseDto()
        {
        }

        public string? ExternalId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public bool IsSuccess { get; set; }
        public string? ConfirmationCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Message { get; set; }

        public string? Provider { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();


    }
}