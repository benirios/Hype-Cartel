using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IProductCatalogService _productCatalog;
    private readonly ApplicationDbContext _db;

    public AdminController(IProductCatalogService productCatalog, ApplicationDbContext db)
    {
        _productCatalog = productCatalog;
        _db = db;
    }

    [HttpGet]
    public IActionResult Produtos()
    {
        var produtos = _productCatalog
            .GetAll()
            .OrderBy(p => p.Id)
            .ToList();
        var categorias = _db.Categories
            .OrderBy(c => c.Name)
            .ToList();

        ViewBag.Categorias = categorias;

        return View(produtos);
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var totalProducts = await _db.Products.CountAsync();
        var totalCategories = await _db.Categories.CountAsync();
        var totalUsers = await _db.Users.CountAsync();
        var totalOrders = await _db.Orders.CountAsync();
        var totalRevenue = await _db.Orders.SumAsync(o => (decimal?)o.Total) ?? 0m;
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var lowStockProducts = await _db.Products.CountAsync(p => p.Stock <= 5);

        var monthlyRevenueRaw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(x => x.Total),
                OrdersCount = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .Take(12)
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

        var vm = new AdminDashboardViewModel
        {
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
                    Revenue = x.Revenue,
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
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(
        string nome,
        decimal preco,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false)
    {
        var produto = new ProdutoViewModel
        {
            Id = _productCatalog.GetNextId(),
            Nome = nome,
            Slug = string.Empty,
            Preco = preco,
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
            return RedirectToAction(nameof(Produtos));
        }

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(
        int id,
        string nome,
        decimal preco,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false)
    {
        var produtoExistente = _productCatalog.GetById(id);
        if (produtoExistente is null)
        {
            TempData["AdminError"] = "Product not found.";
            return RedirectToAction(nameof(Produtos));
        }

        produtoExistente.Nome = nome;
        produtoExistente.Preco = preco;
        produtoExistente.Categoria = categoria;
        produtoExistente.ImagemUrl = imagemUrl;
        produtoExistente.Descricao = descricao;
        produtoExistente.Destaque = destaque;
        produtoExistente.Tamanhos = ParseSizes(tamanhos);

        if (!_productCatalog.Update(produtoExistente, out var error))
        {
            TempData["AdminError"] = error;
            return RedirectToAction(nameof(Produtos));
        }

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int id)
    {
        _productCatalog.Delete(id);
        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CriarCategoria(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            TempData["AdminError"] = "Category name is required.";
            return RedirectToAction(nameof(Produtos));
        }

        var trimmed = nome.Trim();
        var normalized = trimmed.ToLowerInvariant();
        if (_db.Categories.Any(c => c.Name.ToLower() == normalized))
        {
            TempData["AdminError"] = "Category already exists.";
            return RedirectToAction(nameof(Produtos));
        }

        _db.Categories.Add(new Category
        {
            Name = trimmed,
            Slug = Slugify(trimmed)
        });
        _db.SaveChanges();

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarCategoria(int id, string nome)
    {
        var category = _db.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            TempData["AdminError"] = "Category not found.";
            return RedirectToAction(nameof(Produtos));
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            TempData["AdminError"] = "Category name is required.";
            return RedirectToAction(nameof(Produtos));
        }

        var trimmed = nome.Trim();
        var normalized = trimmed.ToLowerInvariant();
        if (_db.Categories.Any(c => c.Id != id && c.Name.ToLower() == normalized))
        {
            TempData["AdminError"] = "Another category already uses this name.";
            return RedirectToAction(nameof(Produtos));
        }

        category.Name = trimmed;
        category.Slug = Slugify(trimmed);
        _db.SaveChanges();

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoverCategoria(int id)
    {
        var category = _db.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            TempData["AdminError"] = "Category not found.";
            return RedirectToAction(nameof(Produtos));
        }

        var hasProducts = _db.Products.Any(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["AdminError"] = "Cannot delete a category with products.";
            return RedirectToAction(nameof(Produtos));
        }

        _db.Categories.Remove(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(Produtos));
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
