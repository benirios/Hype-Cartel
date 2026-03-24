using System.Text.Json;
using MafiaStore.Data;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Services;

public sealed class ProductCatalogEfStore : IProductCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new();
    private readonly ApplicationDbContext _db;

    public ProductCatalogEfStore(ApplicationDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<ProdutoViewModel> GetAll()
    {
        return _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .Select(MapToViewModel)
            .ToList();
    }

    public ProdutoViewModel? GetById(int id)
    {
        var product = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id);

        return product is null ? null : MapToViewModel(product);
    }

    public int GetNextId()
    {
        return _db.Products.Any() ? _db.Products.Max(p => p.Id) + 1 : 1;
    }

    public bool Create(ProdutoViewModel produto, out string error)
    {
        if (!ValidateProduto(produto, out error))
        {
            return false;
        }

        if (_db.Products.Any(p => p.Id == produto.Id))
        {
            error = "Product ID already exists.";
            return false;
        }

        var category = ResolveCategory(produto.Categoria);
        var slug = Slugify(string.IsNullOrWhiteSpace(produto.Slug) ? produto.Nome : produto.Slug);
        if (_db.Products.Any(p => p.Slug == slug))
        {
            error = "Product slug already exists.";
            return false;
        }

        var sku = $"SKU-{produto.Id:000}";
        if (_db.Products.Any(p => p.Sku == sku))
        {
            sku = $"SKU-{produto.Id:000}-{Guid.NewGuid():N}"[..16];
        }

        var entity = new Product
        {
            Id = produto.Id,
            Name = produto.Nome.Trim(),
            Slug = slug,
            Sku = sku,
            Price = produto.Preco,
            Description = produto.Descricao.Trim(),
            ImageUrl = NormalizeAssetPath(produto.ImagemUrl),
            AdditionalImagesJson = JsonSerializer.Serialize(
                produto.ImagensAdicionais
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(NormalizeAssetPath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                JsonOptions),
            SizesJson = JsonSerializer.Serialize(
                produto.Tamanhos
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim().ToUpperInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                JsonOptions),
            Stock = produto.Stock,
            Highlight = produto.Destaque,
            CategoryId = category.Id
        };

        _db.Products.Add(entity);
        _db.SaveChanges();
        error = string.Empty;
        return true;
    }

    public bool Update(ProdutoViewModel produto, out string error)
    {
        if (!ValidateProduto(produto, out error))
        {
            return false;
        }

        var existing = _db.Products.FirstOrDefault(p => p.Id == produto.Id);
        if (existing is null)
        {
            error = "Product not found.";
            return false;
        }

        var nextSlug = Slugify(string.IsNullOrWhiteSpace(produto.Slug) ? produto.Nome : produto.Slug);
        if (_db.Products.Any(p => p.Id != produto.Id && p.Slug == nextSlug))
        {
            error = "Another product already uses this slug.";
            return false;
        }

        var category = ResolveCategory(produto.Categoria);
        existing.Name = produto.Nome.Trim();
        existing.Slug = nextSlug;
        existing.Price = produto.Preco;
        existing.Description = produto.Descricao.Trim();
        existing.ImageUrl = NormalizeAssetPath(produto.ImagemUrl);
        existing.Stock = produto.Stock;
        existing.Highlight = produto.Destaque;
        existing.SizesJson = JsonSerializer.Serialize(
            produto.Tamanhos
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            JsonOptions);
        existing.AdditionalImagesJson = JsonSerializer.Serialize(
            produto.ImagensAdicionais
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(NormalizeAssetPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            JsonOptions);
        existing.CategoryId = category.Id;

        _db.SaveChanges();
        error = string.Empty;
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _db.Products.FirstOrDefault(p => p.Id == id);
        if (existing is null)
        {
            return false;
        }

        _db.Products.Remove(existing);
        _db.SaveChanges();
        return true;
    }

    private Category ResolveCategory(string categoryName)
    {
        var normalizedName = string.IsNullOrWhiteSpace(categoryName) ? "Uncategorized" : categoryName.Trim();
        var normalizedLookup = normalizedName.ToLowerInvariant();
        var category = _db.Categories.FirstOrDefault(c =>
            c.Name.ToLower() == normalizedLookup);

        if (category is not null)
        {
            return category;
        }

        category = new Category
        {
            Name = normalizedName,
            Slug = Slugify(normalizedName)
        };
        _db.Categories.Add(category);
        _db.SaveChanges();
        return category;
    }

    private static ProdutoViewModel MapToViewModel(Product product)
    {
        var sizes = JsonSerializer.Deserialize<List<string>>(product.SizesJson, JsonOptions) ?? new List<string>();
        var extras = JsonSerializer.Deserialize<List<string>>(product.AdditionalImagesJson, JsonOptions) ?? new List<string>();

        return new ProdutoViewModel
        {
            Id = product.Id,
            Nome = product.Name,
            Slug = product.Slug,
            Preco = product.Price,
            Stock = product.Stock,
            ImagemUrl = NormalizeAssetPath(product.ImageUrl),
            Categoria = product.Category?.Name ?? "Uncategorized",
            Descricao = product.Description,
            Destaque = product.Highlight,
            Tamanhos = sizes,
            ImagensAdicionais = extras
        };
    }

    private static bool ValidateProduto(ProdutoViewModel produto, out string error)
    {
        if (produto.Id <= 0)
        {
            error = "Product ID must be greater than zero.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            error = "Product name is required.";
            return false;
        }

        if (produto.Preco < 0)
        {
            error = "Product price cannot be negative.";
            return false;
        }

        if (produto.Stock < 0)
        {
            error = "Product stock cannot be negative.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria))
        {
            error = "Product category is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(produto.ImagemUrl))
        {
            error = "Product image is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(produto.Descricao))
        {
            error = "Product description is required.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string NormalizeAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/catalog/placeholder.svg";
        }

        var trimmed = path.Trim();
        return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
    }

    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "produto";
        }

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
