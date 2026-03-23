using MafiaStore.Models;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        if (!await db.Orders.AnyAsync())
        {
            await SeedSampleOrdersAsync(db);
        }
    }

    private static async Task SeedSampleOrdersAsync(ApplicationDbContext db)
    {
        var products = await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Take(3)
            .ToListAsync();
        if (products.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var first = products[0];
        var firstQuantity = 2;
        var firstSubtotal = first.Price * firstQuantity;
        var firstVat = Math.Round(firstSubtotal * 0.23m, 2, MidpointRounding.AwayFromZero);
        var firstOrder = new Order
        {
            UserId = "seed-customer",
            CreatedAtUtc = new DateTime(now.Year, now.Month, 1, 10, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Completed,
            Subtotal = firstSubtotal,
            Vat = firstVat,
            Total = firstSubtotal + firstVat,
            Lines = new List<OrderLine>
            {
                new()
                {
                    ProductId = first.Id,
                    ProductName = first.Name,
                    Quantity = firstQuantity,
                    Size = "M",
                    UnitPrice = first.Price
                }
            }
        };

        var second = products[Math.Min(1, products.Count - 1)];
        var secondQuantity = 1;
        var secondSubtotal = second.Price * secondQuantity;
        var secondVat = Math.Round(secondSubtotal * 0.23m, 2, MidpointRounding.AwayFromZero);
        var secondOrder = new Order
        {
            UserId = "seed-customer",
            CreatedAtUtc = new DateTime(now.Year, now.Month, 15, 15, 30, 0, DateTimeKind.Utc),
            Status = OrderStatus.Paid,
            Subtotal = secondSubtotal,
            Vat = secondVat,
            Total = secondSubtotal + secondVat,
            Lines = new List<OrderLine>
            {
                new()
                {
                    ProductId = second.Id,
                    ProductName = second.Name,
                    Quantity = secondQuantity,
                    Size = "L",
                    UnitPrice = second.Price
                }
            }
        };

        db.Orders.AddRange(firstOrder, secondOrder);
        await db.SaveChangesAsync();

        db.OrderHistory.AddRange(
            new OrderHistory
            {
                OrderId = firstOrder.Id,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Paid,
                ChangedAtUtc = firstOrder.CreatedAtUtc.AddMinutes(30),
                ChangedBy = "seed-system"
            },
            new OrderHistory
            {
                OrderId = firstOrder.Id,
                FromStatus = OrderStatus.Paid,
                ToStatus = OrderStatus.Shipped,
                ChangedAtUtc = firstOrder.CreatedAtUtc.AddHours(2),
                ChangedBy = "seed-system"
            },
            new OrderHistory
            {
                OrderId = firstOrder.Id,
                FromStatus = OrderStatus.Shipped,
                ToStatus = OrderStatus.Completed,
                ChangedAtUtc = firstOrder.CreatedAtUtc.AddDays(1),
                ChangedBy = "seed-system"
            },
            new OrderHistory
            {
                OrderId = secondOrder.Id,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Paid,
                ChangedAtUtc = secondOrder.CreatedAtUtc.AddMinutes(20),
                ChangedBy = "seed-system"
            });

        await db.SaveChangesAsync();
    }
}
