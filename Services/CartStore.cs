using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Services;

public sealed class CartStore : ICartStore
{
    private readonly ApplicationDbContext _db;
    private readonly IProductCatalogService _productCatalog;

    public CartStore(ApplicationDbContext db, IProductCatalogService productCatalog)
    {
        _db = db;
        _productCatalog = productCatalog;
    }

    public List<CarrinhoItemViewModel> GetItems(string ownerKey)
    {
        var cart = GetOrCreateCart(ownerKey);
        var cartItems = _db.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cart.Id)
            .OrderBy(i => i.Id)
            .ToList();

        var products = _productCatalog.GetAll().ToDictionary(p => p.Id);
        var result = new List<CarrinhoItemViewModel>();
        foreach (var item in cartItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                continue;
            }

            result.Add(new CarrinhoItemViewModel
            {
                Id = product.Id,
                Nome = product.Nome,
                ImagemUrl = product.ImagemUrl,
                Preco = product.Preco,
                Quantidade = item.Quantity,
                Tamanho = item.Size
            });
        }

        return result;
    }

    public int GetCartCount(string ownerKey)
    {
        var cart = GetOrCreateCart(ownerKey);
        return _db.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cart.Id)
            .Sum(i => (int?)i.Quantity) ?? 0;
    }

    public bool AddItem(string ownerKey, int produtoId, string? tamanho)
    {
        var produto = _productCatalog.GetById(produtoId);
        if (produto is null)
        {
            return false;
        }

        var defaultSize = produto.Tamanhos.FirstOrDefault() ?? "M";
        var normalizedSize = NormalizeSize(tamanho, defaultSize);

        var cart = GetOrCreateCart(ownerKey);
        var existing = _db.CartItems.FirstOrDefault(item =>
            item.CartId == cart.Id &&
            item.ProductId == produtoId &&
            item.Size == normalizedSize);

        if (existing is not null)
        {
            existing.Quantity += 1;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = produtoId,
                Size = normalizedSize,
                Quantity = 1
            });
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public bool UpdateQuantity(string ownerKey, int produtoId, string? tamanho, int quantidade)
    {
        var cart = GetOrCreateCart(ownerKey);
        var normalizedSize = NormalizeSize(tamanho);

        var existing = _db.CartItems.FirstOrDefault(item =>
            item.CartId == cart.Id &&
            item.ProductId == produtoId &&
            item.Size == normalizedSize);

        if (existing is null)
        {
            return false;
        }

        if (quantidade <= 0)
        {
            _db.CartItems.Remove(existing);
        }
        else
        {
            existing.Quantity = quantidade;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public bool RemoveItem(string ownerKey, int produtoId, string? tamanho)
    {
        var cart = GetOrCreateCart(ownerKey);
        var normalizedSize = NormalizeSize(tamanho);

        var existing = _db.CartItems.FirstOrDefault(item =>
            item.CartId == cart.Id &&
            item.ProductId == produtoId &&
            item.Size == normalizedSize);

        if (existing is null)
        {
            return false;
        }

        _db.CartItems.Remove(existing);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public bool Clear(string ownerKey)
    {
        var cart = GetOrCreateCart(ownerKey);
        var items = _db.CartItems.Where(i => i.CartId == cart.Id).ToList();
        if (items.Count == 0)
        {
            return true;
        }

        _db.CartItems.RemoveRange(items);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    private Cart GetOrCreateCart(string ownerKey)
    {
        var normalizedOwner = NormalizeOwnerKey(ownerKey);
        var cart = _db.Carts.FirstOrDefault(c => c.OwnerKey == normalizedOwner);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            OwnerKey = normalizedOwner,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.Carts.Add(cart);
        _db.SaveChanges();
        return cart;
    }

    private static string NormalizeOwnerKey(string? ownerKey)
    {
        if (string.IsNullOrWhiteSpace(ownerKey))
        {
            throw new ArgumentException("Cart owner key cannot be empty.", nameof(ownerKey));
        }

        return ownerKey.Trim();
    }

    private static string NormalizeSize(string? size, string fallback = "M")
    {
        return string.IsNullOrWhiteSpace(size)
            ? fallback
            : size.Trim().ToUpperInvariant();
    }
}
