using OrderService.DTO;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Services
{
    public class ProductService : IProductService
    {

        private readonly HttpClient _httpClient;
        private const string productServiceBaseUrl = "http://localhost:8080/api/Product"; 

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult> CancelPurchaseAsync(Guid productId, int quantity)
        {
            try
            {
                var cancelRequest = new
                {
                    quantity = quantity
                };

                var response = await _httpClient.PostAsJsonAsync($"{productServiceBaseUrl}/{productId}/cancel", cancelRequest);
                if (response.IsSuccessStatusCode)
                {
                    return ServiceResult.Ok("Purchase cancelled successfully");
                }
                else
                {
                    return ServiceResult.Fail($"Failed to cancel purchase: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Error cancelling purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CheckProductAvailabilityAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{productServiceBaseUrl}/{productId}/availability");
                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult.Fail($"Failed to check product availability: {response.ReasonPhrase}");
                }

                if(response.Content == null)
                {
                    return ServiceResult.Fail("Product availability response is empty.");
                }
                else
                {
                    AvailabilityInfo? availability = response.Content.ReadFromJsonAsync<AvailabilityInfo>().Result;
                    return ServiceResult.Ok("Product availability checked successfully", availability);
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Error checking product availability: {ex.Message}");
            }
        }

        public async Task<ServiceResult> PurchaseProductAsync(Guid productId, int quantity)
        {
            try
            {
                var purchaseRequest = new 
                {
                    quantity = quantity,
                    priceAtPurchase = 0 
                };
                
                var response = await _httpClient.PostAsJsonAsync($"{productServiceBaseUrl}/{productId}/purchase", purchaseRequest);
                if (response.IsSuccessStatusCode)
                {
                    var purchaseResponse = await response.Content.ReadFromJsonAsync<PurchaseResponseDto>();
                    return ServiceResult.Ok("Product purchased successfully", purchaseResponse);
                }
                else
                {
                    return ServiceResult.Fail($"Failed to purchase product: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Error purchasing product: {ex.Message}");
            }
        }
    }
}
