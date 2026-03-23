using System.Security.Claims;
using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class OrdersAdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersAdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderManageItem
            {
                OrderId = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                Total = o.Total,
                CreatedAtUtc = o.CreatedAtUtc
            })
            .ToListAsync();

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
    {
        var order = await _db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            TempData["OrderAdminError"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!IsValidTransition(order.Status, newStatus))
        {
            TempData["OrderAdminError"] = $"Invalid status transition: {order.Status} → {newStatus}.";
            return RedirectToAction(nameof(Index));
        }

        if (order.Status == newStatus)
        {
            return RedirectToAction(nameof(Index));
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
            var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

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

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var items = await _db.OrderHistory
            .AsNoTracking()
            .OrderByDescending(h => h.ChangedAtUtc)
            .Select(h => new OrderHistoryItem
            {
                OrderId = h.OrderId,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedAtUtc = h.ChangedAtUtc,
                ChangedBy = h.ChangedBy
            })
            .ToListAsync();

        return View(items);
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
}
