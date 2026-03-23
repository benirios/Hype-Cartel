using MafiaStore.Data;
using MafiaStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TopProducts()
    {
        var items = await _db.OrderLines
            .AsNoTracking()
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new TopProductReportItem
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenBy(x => x.ProductName)
            .Take(5)
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyRevenue()
    {
        var raw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(x => (double)x.Total)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        var items = raw.Select(x => new MonthlyRevenueReportItem
        {
            Year = x.Year,
            Month = x.Month,
            Revenue = Convert.ToDecimal(x.Revenue)
        }).ToList();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> OrderStateDistribution()
    {
        var raw = await _db.Orders
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var items = raw
            .Select(x => new OrderStatusDistributionItem
            {
                Status = x.Status.ToString(),
                Count = x.Count
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Status)
            .ToList();

        return View(items);
    }
}
