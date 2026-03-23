using System.Text.Json;
using MafiaStore.Data;
using MafiaStore.Models;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Services;

public sealed class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CheckoutResult> CreateOrderAsync(string userId, string ownerKey)
    {
        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? ownerKey : userId.Trim();

        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerKey == ownerKey);

        if (cart is null || cart.Items.Count == 0)
        {
            return CheckoutResult.Fail("Your cart is empty.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in cart.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    return CheckoutResult.Fail("One or more products no longer exist.");
                }

                if (item.Quantity <= 0)
                {
                    return CheckoutResult.Fail("Invalid cart quantity detected.");
                }

                if (product.Stock < item.Quantity)
                {
                    return CheckoutResult.Fail($"Insufficient stock for {product.Name}.");
                }
            }

            decimal subtotal = 0m;
            var lines = new List<OrderLine>();

            foreach (var item in cart.Items)
            {
                var product = products[item.ProductId];
                product.Stock -= item.Quantity;

                subtotal += product.Price * item.Quantity;
                lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    Size = item.Size,
                    UnitPrice = product.Price
                });
            }

            var vat = Math.Round(subtotal * 0.23m, 2, MidpointRounding.AwayFromZero);
            var total = subtotal + vat;

            var order = new Order
            {
                UserId = normalizedUserId,
                CreatedAtUtc = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                Vat = vat,
                Total = total,
                Lines = lines
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return CheckoutResult.Ok(order.Id);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
