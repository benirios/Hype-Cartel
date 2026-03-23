using MafiaStore.Models;

namespace MafiaStore.Models.ViewModels;

public sealed class TopProductReportItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public sealed class MonthlyRevenueReportItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class OrderStatusDistributionItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class OrderManageItem
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class OrderHistoryItem
{
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
