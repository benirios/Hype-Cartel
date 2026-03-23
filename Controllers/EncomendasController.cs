using System.Security.Claims;
using MafiaStore.Data;
using MafiaStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Controllers;

[Authorize]
public class EncomendasController : Controller
{
    private readonly ApplicationDbContext _db;

    public EncomendasController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new UserOrderViewModel
            {
                Id = o.Id,
                CreatedAtUtc = o.CreatedAtUtc,
                Status = o.Status,
                Subtotal = o.Subtotal,
                Vat = o.Vat,
                Total = o.Total
            })
            .ToListAsync();

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null)
        {
            return NotFound();
        }

        var vm = new UserOrderViewModel
        {
            Id = order.Id,
            CreatedAtUtc = order.CreatedAtUtc,
            Status = order.Status,
            Subtotal = order.Subtotal,
            Vat = order.Vat,
            Total = order.Total,
            Lines = order.Lines
                .OrderBy(l => l.Id)
                .Select(l => new UserOrderLineViewModel
                {
                    ProductName = l.ProductName,
                    Size = l.Size,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                })
                .ToList()
        };

        return View(vm);
    }
}
