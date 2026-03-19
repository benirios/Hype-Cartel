using MafiaStore.Models.ViewModels;

namespace MafiaStore.Services;

public sealed class CartStore : ICartStore
{
    private readonly IProductCatalogService _productCatalog;
    private readonly object _syncRoot = new();

    private readonly List<CarrinhoItemViewModel> _items = new()
    {
        new()
        {
            Id = 1,
            Nome = "Shadow Overcoat",
            ImagemUrl = "/catalog/shadow-overcoat.svg",
            Preco = 349m,
            Quantidade = 1,
            Tamanho = "L"
        },
        new()
        {
            Id = 5,
            Nome = "Silk Noir Shirt",
            ImagemUrl = "/catalog/silk-noir-shirt.svg",
            Preco = 189m,
            Quantidade = 2,
            Tamanho = "M"
        },
        new()
        {
            Id = 11,
            Nome = "Gold Signet Ring",
            ImagemUrl = "/catalog/gold-signet-ring.svg",
            Preco = 129m,
            Quantidade = 1,
            Tamanho = "M"
        }
    };

    public CartStore(IProductCatalogService productCatalog)
    {
        _productCatalog = productCatalog;
    }

    public List<CarrinhoItemViewModel> GetItems()
    {
        lock (_syncRoot)
        {
            return _items
                .Select(item => new CarrinhoItemViewModel
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    ImagemUrl = item.ImagemUrl,
                    Preco = item.Preco,
                    Quantidade = item.Quantidade,
                    Tamanho = item.Tamanho
                })
                .ToList();
        }
    }

    public int GetCartCount()
    {
        lock (_syncRoot)
        {
            return _items.Sum(item => item.Quantidade);
        }
    }

    public bool AddItem(int produtoId, string? tamanho)
    {
        var produto = _productCatalog.GetById(produtoId);
        if (produto is null)
        {
            return false;
        }

        var defaultSize = produto.Tamanhos.FirstOrDefault() ?? "M";
        var normalizedSize = NormalizeSize(tamanho, defaultSize);

        lock (_syncRoot)
        {
            var existing = _items.FirstOrDefault(item =>
                item.Id == produtoId &&
                item.Tamanho.Equals(normalizedSize, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                existing.Quantidade++;
                return true;
            }

            _items.Add(new CarrinhoItemViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                ImagemUrl = produto.ImagemUrl,
                Preco = produto.Preco,
                Quantidade = 1,
                Tamanho = normalizedSize
            });

            return true;
        }
    }

    public bool UpdateQuantity(int produtoId, string? tamanho, int quantidade)
    {
        var normalizedSize = NormalizeSize(tamanho);

        lock (_syncRoot)
        {
            var existing = _items.FirstOrDefault(item =>
                item.Id == produtoId &&
                item.Tamanho.Equals(normalizedSize, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return false;
            }

            if (quantidade <= 0)
            {
                _items.Remove(existing);
                return true;
            }

            existing.Quantidade = quantidade;
            return true;
        }
    }

    public bool RemoveItem(int produtoId, string? tamanho)
    {
        var normalizedSize = NormalizeSize(tamanho);

        lock (_syncRoot)
        {
            var existing = _items.FirstOrDefault(item =>
                item.Id == produtoId &&
                item.Tamanho.Equals(normalizedSize, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return false;
            }

            _items.Remove(existing);
            return true;
        }
    }

    private static string NormalizeSize(string? size, string fallback = "M")
    {
        return string.IsNullOrWhiteSpace(size)
            ? fallback
            : size.Trim().ToUpperInvariant();
    }
}
