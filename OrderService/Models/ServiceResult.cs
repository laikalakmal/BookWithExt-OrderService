namespace OrderService.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public static ServiceResult Ok(string message = "Operation successful", object? data = null)
        {
            return new ServiceResult { Success = true, Message = message, Data = data };
        }

        public static ServiceResult Fail(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }
}