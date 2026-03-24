using System.Security.Claims;
using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly HashSet<string> AllowedTabs = new(StringComparer.OrdinalIgnoreCase)
    {
        "overview",
        "products",
        "categories",
        "orders",
        "users",
        "reports"
    };
    private static readonly SemaphoreSlim UserAdminMutationLock = new(1, 1);

    private readonly IProductCatalogService _productCatalog;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        IProductCatalogService productCatalog,
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _productCatalog = productCatalog;
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Produtos()
    {
        return RedirectToAction(nameof(Dashboard), new { tab = "products" });
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(string? tab = null, string? userSearch = null, int? orderId = null)
    {
        var activeTab = NormalizeTab(tab);
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var totalProducts = await _db.Products.CountAsync();
        var totalCategories = await _db.Categories.CountAsync();
        var totalUsers = await _db.Users.CountAsync();
        var totalOrders = await _db.Orders.CountAsync();
        var totalRevenueRaw = await _db.Orders.SumAsync(o => (double?)o.Total) ?? 0d;
        var totalRevenue = decimal.Round((decimal)totalRevenueRaw, 2, MidpointRounding.AwayFromZero);
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var lowStockProducts = await _db.Products.CountAsync(p => p.Stock <= 5);

        var monthlyRevenueRaw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(x => (double?)x.Total) ?? 0d,
                OrdersCount = g.Count()
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(12)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        var orderStatusRaw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var topProductsRaw = await _db.OrderLines
            .AsNoTracking()
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenBy(x => x.ProductName)
            .Take(5)
            .ToListAsync();

        var recentOrdersRaw = await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(8)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.Status,
                o.Total,
                o.CreatedAtUtc
            })
            .ToListAsync();

        var orders = await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(100)
            .Select(o => new OrderManageItem
            {
                OrderId = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                Total = o.Total,
                CreatedAtUtc = o.CreatedAtUtc
            })
            .ToListAsync();

        var orderHistory = await _db.OrderHistory
            .AsNoTracking()
            .OrderByDescending(h => h.ChangedAtUtc)
            .Take(100)
            .Select(h => new OrderHistoryItem
            {
                OrderId = h.OrderId,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedAtUtc = h.ChangedAtUtc,
                ChangedBy = h.ChangedBy
            })
            .ToListAsync();

        var selectedOrder = await LoadOrderDetailAsync(orderId);

        var products = _productCatalog
            .GetAll()
            .OrderBy(p => p.Id)
            .ToList();

        var categories = await _db.Categories
            .AsNoTracking()
            .GroupJoin(
                _db.Products.AsNoTracking(),
                category => category.Id,
                product => product.CategoryId,
                (category, categoryProducts) => new DashboardCategoryManageItem
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    ProductCount = categoryProducts.Count()
                })
            .OrderBy(c => c.Name)
            .ToListAsync();

        var normalizedUserSearch = userSearch?.Trim() ?? string.Empty;
        var usersQuery = _userManager.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(normalizedUserSearch))
        {
            var search = normalizedUserSearch.ToLowerInvariant();
            usersQuery = usersQuery.Where(u =>
                (u.UserName ?? string.Empty).ToLower().Contains(search) ||
                (u.Email ?? string.Empty).ToLower().Contains(search));
        }

        var usersRaw = await usersQuery
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.LockoutEnd
            })
            .Take(300)
            .ToListAsync();

        var roleRows = await _db.UserRoles
            .AsNoTracking()
            .Join(
                _db.Roles.AsNoTracking(),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new
                {
                    userRole.UserId,
                    RoleName = role.Name ?? "Customer"
                })
            .ToListAsync();

        var roleByUserId = roleRows
            .GroupBy(r => r.UserId)
            .ToDictionary(
                g => g.Key,
                g =>
                    g.Select(x => x.RoleName)
                        .FirstOrDefault(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase))
                    ?? g.Select(x => x.RoleName).FirstOrDefault()
                    ?? "Customer");

        var users = usersRaw
            .Select(user => new DashboardUserManageItem
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roleByUserId.TryGetValue(user.Id, out var role) ? role : "Customer",
                IsActive = IsUserActive(user.LockoutEnd),
                LockoutEnd = user.LockoutEnd
            })
            .ToList();

        var vm = new AdminDashboardViewModel
        {
            ActiveTab = activeTab,
            UserSearch = normalizedUserSearch,
            CurrentUserId = currentUserId,
            TotalProducts = totalProducts,
            TotalCategories = totalCategories,
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStockProducts,
            MonthlyRevenue = monthlyRevenueRaw
                .Select(x => new DashboardMonthlyRevenueItem
                {
                    Year = x.Year,
                    Month = x.Month,
                    Revenue = decimal.Round((decimal)x.Revenue, 2, MidpointRounding.AwayFromZero),
                    OrdersCount = x.OrdersCount
                })
                .ToList(),
            OrdersByStatus = orderStatusRaw
                .Select(x => new DashboardOrderStatusItem
                {
                    Status = x.Status.ToString(),
                    Count = x.Count
                })
                .ToList(),
            TopProducts = topProductsRaw
                .Select(x => new DashboardTopProductItem
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    QuantitySold = x.QuantitySold
                })
                .ToList(),
            RecentOrders = recentOrdersRaw
                .Select(x => new DashboardRecentOrderItem
                {
                    OrderId = x.Id,
                    UserId = x.UserId,
                    Status = x.Status.ToString(),
                    Total = x.Total,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList(),
            Products = products,
            Categories = categories,
            Orders = orders,
            OrderHistory = orderHistory,
            Users = users,
            SelectedOrder = selectedOrder
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(
        string nome,
        decimal preco,
        int stock,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false,
        string activeTab = "products")
    {
        var produto = new ProdutoViewModel
        {
            Id = _productCatalog.GetNextId(),
            Nome = nome,
            Slug = string.Empty,
            Preco = preco,
            Stock = stock,
            Categoria = categoria,
            ImagemUrl = imagemUrl,
            Descricao = descricao,
            Destaque = destaque,
            Tamanhos = ParseSizes(tamanhos),
            ImagensAdicionais = new List<string>()
        };

        if (!_productCatalog.Create(produto, out var error))
        {
            TempData["AdminError"] = error;
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        TempData["AdminSuccess"] = $"Product '{nome}' created successfully.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(
        int id,
        string nome,
        decimal preco,
        int stock,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false,
        string activeTab = "products")
    {
        var produtoExistente = _productCatalog.GetById(id);
        if (produtoExistente is null)
        {
            TempData["AdminError"] = "Product not found.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        produtoExistente.Nome = nome;
        produtoExistente.Preco = preco;
        produtoExistente.Stock = stock;
        produtoExistente.Categoria = categoria;
        produtoExistente.ImagemUrl = imagemUrl;
        produtoExistente.Descricao = descricao;
        produtoExistente.Destaque = destaque;
        produtoExistente.Tamanhos = ParseSizes(tamanhos);

        if (!_productCatalog.Update(produtoExistente, out var error))
        {
            TempData["AdminError"] = error;
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        TempData["AdminSuccess"] = $"Product #{id} updated successfully.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int id, string activeTab = "products")
    {
        if (!_productCatalog.Delete(id))
        {
            TempData["AdminError"] = "Product not found.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        TempData["AdminSuccess"] = $"Product #{id} deleted.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CriarCategoria(string nome, string activeTab = "categories")
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            TempData["AdminError"] = "Category name is required.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        var trimmed = nome.Trim();
        var normalized = trimmed.ToLowerInvariant();
        if (_db.Categories.Any(c => c.Name.ToLower() == normalized))
        {
            TempData["AdminError"] = "Category already exists.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        _db.Categories.Add(new Category
        {
            Name = trimmed,
            Slug = Slugify(trimmed)
        });
        _db.SaveChanges();

        TempData["AdminSuccess"] = $"Category '{trimmed}' created.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarCategoria(int id, string nome, string activeTab = "categories")
    {
        var category = _db.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            TempData["AdminError"] = "Category not found.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            TempData["AdminError"] = "Category name is required.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        var trimmed = nome.Trim();
        var normalized = trimmed.ToLowerInvariant();
        if (_db.Categories.Any(c => c.Id != id && c.Name.ToLower() == normalized))
        {
            TempData["AdminError"] = "Another category already uses this name.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        category.Name = trimmed;
        category.Slug = Slugify(trimmed);
        _db.SaveChanges();

        TempData["AdminSuccess"] = $"Category #{id} updated.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoverCategoria(int id, string activeTab = "categories")
    {
        var category = _db.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            TempData["AdminError"] = "Category not found.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        var hasProducts = _db.Products.Any(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["AdminError"] = "Cannot delete a category with products.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        _db.Categories.Remove(category);
        _db.SaveChanges();

        TempData["AdminSuccess"] = $"Category #{id} deleted.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus, string activeTab = "orders")
    {
        var order = await _db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            TempData["AdminError"] = "Order not found.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        if (!IsValidTransition(order.Status, newStatus))
        {
            TempData["AdminError"] = $"Invalid status transition: {order.Status} -> {newStatus}.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab) });
        }

        if (order.Status == newStatus)
        {
            TempData["AdminSuccess"] = $"Order #{orderId} already in status {newStatus}.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), orderId });
        }

        var changedBy = User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.Identity?.Name
            ?? "admin";

        var history = new OrderHistory
        {
            OrderId = order.Id,
            FromStatus = order.Status,
            ToStatus = newStatus,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = changedBy
        };

        var isCancellingFromPaidLikeState =
            newStatus == OrderStatus.Cancelled &&
            order.Status is OrderStatus.Paid or OrderStatus.Shipped;

        if (isCancellingFromPaidLikeState)
        {
            var productIds = order.Lines.Select(line => line.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);

            foreach (var line in order.Lines)
            {
                if (products.TryGetValue(line.ProductId, out var product))
                {
                    product.Stock += line.Quantity;
                }
            }
        }

        order.Status = newStatus;
        _db.OrderHistory.Add(history);
        await _db.SaveChangesAsync();

        TempData["AdminSuccess"] = $"Order #{orderId} status updated to {newStatus}.";
        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(string userId, string role, string activeTab = "users", string? userSearch = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["AdminError"] = "User is required.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
        }

        var normalizedRole = NormalizeManagedRole(role);
        if (normalizedRole is null)
        {
            TempData["AdminError"] = "Invalid role. Allowed values: Admin or Customer.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
        }

        await UserAdminMutationLock.WaitAsync();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                TempData["AdminError"] = "User not found.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                TempData["AdminError"] = $"Role '{normalizedRole}' does not exist.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentlyAdmin = currentRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
            var userIsActive = IsUserActive(user.LockoutEnd);
            var targetIsAdmin = string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isDemotingAdmin = currentlyAdmin && !targetIsAdmin;
            var managedRoles = currentRoles
                .Where(IsManagedRole)
                .ToList();

            var alreadyOnlyTargetRole = managedRoles.Count == 1 &&
                string.Equals(managedRoles[0], normalizedRole, StringComparison.OrdinalIgnoreCase);
            if (alreadyOnlyTargetRole)
            {
                TempData["AdminSuccess"] = $"User '{user.UserName}' is already in role {normalizedRole}.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (isDemotingAdmin)
                {
                    var adminUsersCount = await CountAdminUsersAsync();
                    if (adminUsersCount <= 1)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = "Cannot remove Admin role from the last admin account.";
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }

                    if (userIsActive)
                    {
                        var activeAdminUsersCount = await CountActiveAdminUsersAsync();
                        if (activeAdminUsersCount <= 1)
                        {
                            await tx.RollbackAsync();
                            TempData["AdminError"] = "Cannot remove Admin role from the last active admin account.";
                            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                        }
                    }
                }

                if (managedRoles.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, managedRoles);
                    if (!removeResult.Succeeded)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }
                }

                var rolesAfterRemoval = managedRoles.Count > 0
                    ? await _userManager.GetRolesAsync(user)
                    : currentRoles;

                if (!rolesAfterRemoval.Any(r => string.Equals(r, normalizedRole, StringComparison.OrdinalIgnoreCase)))
                {
                    var addResult = await _userManager.AddToRoleAsync(user, normalizedRole);
                    if (!addResult.Succeeded)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = string.Join("; ", addResult.Errors.Select(e => e.Description));
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }
                }

                if (isDemotingAdmin)
                {
                    var adminUsersCountAfter = await CountAdminUsersAsync();
                    if (adminUsersCountAfter <= 0)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = "Operation blocked to prevent removing the last admin account.";
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }

                    if (userIsActive)
                    {
                        var activeAdminUsersCountAfter = await CountActiveAdminUsersAsync();
                        if (activeAdminUsersCountAfter <= 0)
                        {
                            await tx.RollbackAsync();
                            TempData["AdminError"] = "Operation blocked to prevent removing the last active admin account.";
                            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                        }
                    }
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            TempData["AdminSuccess"] = $"Role for user '{user.UserName}' updated to {normalizedRole}.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
        }
        catch
        {
            throw;
        }
        finally
        {
            UserAdminMutationLock.Release();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(string userId, bool activate, string activeTab = "users", string? userSearch = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["AdminError"] = "User is required.";
            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
        }

        await UserAdminMutationLock.WaitAsync();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                TempData["AdminError"] = "User not found.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            var userIsActive = IsUserActive(user.LockoutEnd);
            if (activate == userIsActive)
            {
                TempData["AdminSuccess"] = activate
                    ? $"User '{user.UserName}' is already active."
                    : $"User '{user.UserName}' is already inactive.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!activate && string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
            {
                TempData["AdminError"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isDeactivatingActiveAdmin = !activate && userIsActive && isAdmin;
                if (isDeactivatingActiveAdmin)
                {
                    var adminUsersCount = await CountAdminUsersAsync();
                    if (adminUsersCount <= 1)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = "Cannot deactivate the last admin account.";
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }

                    var activeAdminUsersCount = await CountActiveAdminUsersAsync();
                    if (activeAdminUsersCount <= 1)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = "Cannot deactivate the last active admin account.";
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }
                }

                user.LockoutEnabled = true;
                user.LockoutEnd = activate
                    ? DateTimeOffset.UtcNow.AddMinutes(-1)
                    : DateTimeOffset.UtcNow.AddYears(100);

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    TempData["AdminError"] = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                }

                if (isDeactivatingActiveAdmin)
                {
                    var activeAdminUsersCountAfter = await CountActiveAdminUsersAsync();
                    if (activeAdminUsersCountAfter <= 0)
                    {
                        await tx.RollbackAsync();
                        TempData["AdminError"] = "Operation blocked to prevent deactivating the last active admin account.";
                        return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
                    }
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            TempData["AdminSuccess"] = activate
                ? $"User '{user.UserName}' activated."
                : $"User '{user.UserName}' deactivated.";

            return RedirectToAction(nameof(Dashboard), new { tab = NormalizeTab(activeTab), userSearch });
        }
        catch
        {
            throw;
        }
        finally
        {
            UserAdminMutationLock.Release();
        }
    }

    private async Task<UserOrderViewModel?> LoadOrderDetailAsync(int? orderId)
    {
        if (!orderId.HasValue || orderId.Value <= 0)
        {
            return null;
        }

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId.Value);
        if (order is null)
        {
            return null;
        }

        return new UserOrderViewModel
        {
            Id = order.Id,
            CreatedAtUtc = order.CreatedAtUtc,
            Status = order.Status,
            Subtotal = order.Subtotal,
            Vat = order.Vat,
            Total = order.Total,
            Lines = order.Lines
                .OrderBy(line => line.Id)
                .Select(line => new UserOrderLineViewModel
                {
                    ProductName = line.ProductName,
                    Size = line.Size,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                })
                .ToList()
        };
    }

    private static bool IsUserActive(DateTimeOffset? lockoutEnd)
    {
        return !lockoutEnd.HasValue || lockoutEnd.Value <= DateTimeOffset.UtcNow;
    }

    private static bool IsManagedRole(string roleName)
    {
        return string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(roleName, "Customer", StringComparison.OrdinalIgnoreCase);
    }

    private Task<int> CountAdminUsersAsync()
    {
        return _db.UserRoles
            .AsNoTracking()
            .Join(
                _db.Roles.AsNoTracking().Where(role => role.NormalizedName == "ADMIN"),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, _) => userRole.UserId)
            .Distinct()
            .CountAsync();
    }

    private Task<int> CountActiveAdminUsersAsync()
    {
        var now = DateTimeOffset.UtcNow;

        return _db.UserRoles
            .AsNoTracking()
            .Join(
                _db.Roles.AsNoTracking().Where(role => role.NormalizedName == "ADMIN"),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, _) => userRole.UserId)
            .Join(
                _db.Users.AsNoTracking(),
                userId => userId,
                user => user.Id,
                (userId, user) => new
                {
                    userId,
                    user.LockoutEnd
                })
            .Where(x => !x.LockoutEnd.HasValue || x.LockoutEnd <= now)
            .Select(x => x.userId)
            .Distinct()
            .CountAsync();
    }

    private static string? NormalizeManagedRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "admin" => "Admin",
            "customer" => "Customer",
            _ => null
        };
    }

    private static string NormalizeTab(string? tab)
    {
        if (string.IsNullOrWhiteSpace(tab))
        {
            return "overview";
        }

        var value = tab.Trim().ToLowerInvariant();
        return AllowedTabs.Contains(value) ? value : "overview";
    }

    private static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return from switch
        {
            OrderStatus.Pending => to is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => to is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => to is OrderStatus.Completed,
            OrderStatus.Cancelled => false,
            OrderStatus.Completed => false,
            _ => false
        };
    }

    private static List<string> ParseSizes(string? sizes)
    {
        if (string.IsNullOrWhiteSpace(sizes))
        {
            return new List<string>();
        }

        return sizes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(size => !string.IsNullOrWhiteSpace(size))
            .Select(size => size.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }
}
