using MafiaStore.Controllers;
using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class IntegrationFlowTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;

    public IntegrationFlowTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ApplicationDbContext(dbOptions);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task CheckoutSuccess_DecrementsStock_CreatesOrder_ClearsCart()
    {
        var category = new Category { Name = "Shirts", Slug = "shirts" };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var product = new Product
        {
            Id = 101,
            Name = "Shirt",
            Slug = "shirt",
            Sku = "SKU-101",
            Price = 100m,
            Description = "desc",
            ImageUrl = "/catalog/shirt.png",
            Stock = 5,
            CategoryId = category.Id
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var cart = new Cart { OwnerKey = "owner:1", UpdatedAtUtc = DateTime.UtcNow };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        _db.CartItems.Add(new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = 2,
            Size = "M"
        });
        await _db.SaveChangesAsync();

        var service = new OrderService(_db);
        var result = await service.CreateOrderAsync("user-1", "owner:1");

        Assert.True(result.Success);
        Assert.NotNull(result.OrderId);

        var dbProduct = await _db.Products.SingleAsync(p => p.Id == product.Id);
        Assert.Equal(3, dbProduct.Stock);

        var order = await _db.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == result.OrderId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Lines);

        var remainingItems = await _db.CartItems.Where(i => i.CartId == cart.Id).CountAsync();
        Assert.Equal(0, remainingItems);
    }

    [Fact]
    public async Task CheckoutFailure_WithInsufficientStock_DoesNotCreateOrder()
    {
        var category = new Category { Name = "Outerwear", Slug = "outerwear" };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var product = new Product
        {
            Id = 202,
            Name = "Coat",
            Slug = "coat",
            Sku = "SKU-202",
            Price = 300m,
            Description = "desc",
            ImageUrl = "/catalog/coat.png",
            Stock = 1,
            CategoryId = category.Id
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var cart = new Cart { OwnerKey = "owner:2", UpdatedAtUtc = DateTime.UtcNow };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        _db.CartItems.Add(new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = 2,
            Size = "L"
        });
        await _db.SaveChangesAsync();

        var service = new OrderService(_db);
        var result = await service.CreateOrderAsync("user-2", "owner:2");

        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.ErrorMessage ?? string.Empty);
        Assert.Equal(0, await _db.Orders.CountAsync());
        Assert.Equal(1, (await _db.Products.SingleAsync(p => p.Id == product.Id)).Stock);
    }

    [Fact]
    public void AdminProductCrud_CreatesUpdatesAndDeletes()
    {
        _db.Categories.Add(new Category { Name = "Accessories", Slug = "accessories" });
        _db.SaveChanges();

        var catalog = new ProductCatalogEfStore(_db);
        var controller = new AdminController(catalog, _db);

        var createResult = controller.Criar(
            nome: "Ring",
            preco: 99m,
            categoria: "Accessories",
            imagemUrl: "/catalog/ring.png",
            descricao: "Silver ring",
            tamanhos: "M,L",
            destaque: false);
        Assert.IsType<RedirectToActionResult>(createResult);
        Assert.Equal(1, _db.Products.Count());

        var editResult = controller.Editar(
            id: 1,
            nome: "Ring Updated",
            preco: 109m,
            categoria: "Accessories",
            imagemUrl: "/catalog/ring-new.png",
            descricao: "Updated",
            tamanhos: "S,M",
            destaque: true);
        Assert.IsType<RedirectToActionResult>(editResult);
        Assert.Equal("Ring Updated", _db.Products.Single().Name);

        var deleteResult = controller.Remover(1);
        Assert.IsType<RedirectToActionResult>(deleteResult);
        Assert.Equal(0, _db.Products.Count());
    }

    [Fact]
    public async Task ReportQueries_ReturnExpectedAggregates()
    {
        _db.Orders.AddRange(
            new Order
            {
                UserId = "u1",
                CreatedAtUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = OrderStatus.Pending,
                Subtotal = 100,
                Vat = 23,
                Total = 123
            },
            new Order
            {
                UserId = "u2",
                CreatedAtUtc = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = OrderStatus.Paid,
                Subtotal = 200,
                Vat = 46,
                Total = 246
            });
        await _db.SaveChangesAsync();

        var firstOrderId = _db.Orders.OrderBy(o => o.Id).First().Id;
        var secondOrderId = _db.Orders.OrderBy(o => o.Id).Skip(1).First().Id;
        _db.OrderLines.AddRange(
            new OrderLine
            {
                OrderId = firstOrderId,
                ProductId = 1,
                ProductName = "A",
                Quantity = 2,
                UnitPrice = 50m
            },
            new OrderLine
            {
                OrderId = secondOrderId,
                ProductId = 2,
                ProductName = "B",
                Quantity = 5,
                UnitPrice = 40m
            });
        await _db.SaveChangesAsync();

        var controller = new ReportsController(_db);

        var topProductsResult = await controller.TopProducts() as ViewResult;
        Assert.NotNull(topProductsResult);
        var topProducts = Assert.IsType<List<TopProductReportItem>>(topProductsResult!.Model);
        Assert.Equal("B", topProducts.First().ProductName);

        var revenueResult = await controller.MonthlyRevenue() as ViewResult;
        Assert.NotNull(revenueResult);
        var revenue = Assert.IsType<List<MonthlyRevenueReportItem>>(revenueResult!.Model);
        Assert.Equal(2, revenue.Count);
        Assert.Contains(revenue, x => x.Month == 1 && x.Revenue == 123m);
        Assert.Contains(revenue, x => x.Month == 2 && x.Revenue == 246m);

        var distributionResult = await controller.OrderStateDistribution() as ViewResult;
        Assert.NotNull(distributionResult);
        var distribution = Assert.IsType<List<OrderStatusDistributionItem>>(distributionResult!.Model);
        Assert.Contains(distribution, x => x.Status == "Pending" && x.Count == 1);
        Assert.Contains(distribution, x => x.Status == "Paid" && x.Count == 1);
    }

    [Fact]
    public async Task CancellingPaidOrder_ReplenishesStock()
    {
        var category = new Category { Name = "Accessories", Slug = "accessories" };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var product = new Product
        {
            Id = 303,
            Name = "Bracelet",
            Slug = "bracelet",
            Sku = "SKU-303",
            Price = 50m,
            Description = "desc",
            ImageUrl = "/catalog/bracelet.png",
            Stock = 2,
            CategoryId = category.Id
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            UserId = "u-cancel",
            CreatedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Paid,
            Subtotal = 150m,
            Vat = 34.5m,
            Total = 184.5m,
            Lines = new List<OrderLine>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 3,
                    Size = "M",
                    UnitPrice = 50m
                }
            }
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var controller = new OrdersAdminController(_db);
        var result = await controller.UpdateStatus(order.Id, OrderStatus.Cancelled);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(OrderStatus.Cancelled, (await _db.Orders.SingleAsync(o => o.Id == order.Id)).Status);
        Assert.Equal(5, (await _db.Products.SingleAsync(p => p.Id == product.Id)).Stock);
        Assert.True(await _db.OrderHistory.AnyAsync(h => h.OrderId == order.Id && h.ToStatus == OrderStatus.Cancelled));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
