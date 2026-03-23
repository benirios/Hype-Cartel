namespace MafiaStore.Models.ViewModels;

public sealed class UserOrderViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public List<UserOrderLineViewModel> Lines { get; set; } = new();
}

public sealed class UserOrderLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}
