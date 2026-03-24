namespace MafiaStore.Models.ViewModels;

public sealed class AdminDashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }

    public decimal AverageOrderValue => TotalOrders == 0 ? 0m : TotalRevenue / TotalOrders;

    public string ActiveTab { get; set; } = "overview";
    public string UserSearch { get; set; } = string.Empty;
    public string CurrentUserId { get; set; } = string.Empty;

    public List<DashboardMonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    public List<DashboardOrderStatusItem> OrdersByStatus { get; set; } = new();
    public List<DashboardTopProductItem> TopProducts { get; set; } = new();
    public List<DashboardRecentOrderItem> RecentOrders { get; set; } = new();
    public List<ProdutoViewModel> Products { get; set; } = new();
    public List<DashboardCategoryManageItem> Categories { get; set; } = new();
    public List<OrderManageItem> Orders { get; set; } = new();
    public List<OrderHistoryItem> OrderHistory { get; set; } = new();
    public List<DashboardUserManageItem> Users { get; set; } = new();
    public UserOrderViewModel? SelectedOrder { get; set; }
}

public sealed class DashboardMonthlyRevenueItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}

public sealed class DashboardOrderStatusItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class DashboardTopProductItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public sealed class DashboardRecentOrderItem
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class DashboardCategoryManageItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public sealed class DashboardUserManageItem
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}
