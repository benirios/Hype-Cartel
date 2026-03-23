namespace MafiaStore.Services;

public interface IOrderService
{
    Task<CheckoutResult> CreateOrderAsync(string userId, string ownerKey);
}

public sealed class CheckoutResult
{
    public bool Success { get; init; }
    public int? OrderId { get; init; }
    public string? ErrorMessage { get; init; }

    public static CheckoutResult Ok(int orderId) => new() { Success = true, OrderId = orderId };
    public static CheckoutResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
