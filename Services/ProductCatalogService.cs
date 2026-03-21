using System.Text.Json;
using MafiaStore.Models.ViewModels;

namespace MafiaStore.Services;

public sealed class ProductCatalogService : IProductCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true
    };

    private readonly string _productsPath;
    private readonly object _syncRoot = new();
    private List<ProdutoViewModel>? _cache;

    public ProductCatalogService(IWebHostEnvironment environment)
    {
        _productsPath = Path.Combine(environment.ContentRootPath, "Catalog_Assets", "products.json");
    }

    public IReadOnlyList<ProdutoViewModel> GetAll()
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            return _cache!
                .Select(CloneProduto)
                .ToList();
        }
    }

    public ProdutoViewModel? GetById(int id)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var produto = _cache!.FirstOrDefault(p => p.Id == id);
            return produto is null ? null : CloneProduto(produto);
        }
    }

    public int GetNextId()
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            return _cache!.Count == 0 ? 1 : _cache.Max(p => p.Id) + 1;
        }
    }

    public bool Create(ProdutoViewModel produto, out string error)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var products = _cache!;

            if (!ValidateProduto(produto, out error))
            {
                return false;
            }

            if (products.Any(p => p.Id == produto.Id))
            {
                error = "Product ID already exists.";
                return false;
            }

            products.Add(CloneProduto(produto));
            Persist();
            return true;
        }
    }

    public bool Update(ProdutoViewModel produto, out string error)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var products = _cache!;

            if (!ValidateProduto(produto, out error))
            {
                return false;
            }

            var existing = products.FirstOrDefault(p => p.Id == produto.Id);
            if (existing is null)
            {
                error = "Product not found.";
                return false;
            }

            existing.Nome = produto.Nome.Trim();
            existing.Slug = string.IsNullOrWhiteSpace(produto.Slug)
                ? Slugify(produto.Nome)
                : Slugify(produto.Slug);
            existing.Preco = produto.Preco;
            existing.ImagemUrl = NormalizeAssetPath(produto.ImagemUrl);
            existing.Categoria = produto.Categoria.Trim();
            existing.Descricao = produto.Descricao.Trim();
            existing.Destaque = produto.Destaque;
            existing.Tamanhos = produto.Tamanhos
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            existing.ImagensAdicionais = produto.ImagensAdicionais
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(NormalizeAssetPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Persist();
            error = string.Empty;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var products = _cache!;
            var existing = products.FirstOrDefault(p => p.Id == id);
            if (existing is null)
            {
                return false;
            }

            products.Remove(existing);
            Persist();
            return true;
        }
    }

    private void EnsureLoaded()
    {
        if (_cache is not null)
        {
            return;
        }

        if (!File.Exists(_productsPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_productsPath)!);
            _cache = BuildDefaultProducts();
            Persist();
            return;
        }

        var json = File.ReadAllText(_productsPath);
        var products = JsonSerializer.Deserialize<List<ProdutoJson>>(json, JsonOptions) ?? new List<ProdutoJson>();
        _cache = products
            .Where(p => int.TryParse(p.Id, out _))
            .Select(MapFromJson)
            .ToList();

        if (_cache.Count == 0)
        {
            _cache = BuildDefaultProducts();
            Persist();
        }
    }

    private void Persist()
    {
        var payload = _cache!
            .Select(MapToJson)
            .ToList();

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var directory = Path.GetDirectoryName(_productsPath)!;
        Directory.CreateDirectory(directory);

        var tempPath = _productsPath + ".tmp";
        File.WriteAllText(tempPath, json);

        if (File.Exists(_productsPath))
        {
            File.Delete(_productsPath);
        }

        File.Move(tempPath, _productsPath);
    }

    private static ProdutoViewModel MapFromJson(ProdutoJson source)
    {
        _ = int.TryParse(source.Id, out var id);

        var images = source.Images ?? new List<string>();
        var primaryImage = images.FirstOrDefault() ?? "/catalog/placeholder.svg";
        var extras = images.Skip(1).ToList();

        var categoria = MapCategoryIdToName(source.CategoryId);

        return new ProdutoViewModel
        {
            Id = id,
            Nome = source.Name ?? "Untitled Product",
            Slug = Slugify(string.IsNullOrWhiteSpace(source.Slug) ? source.Name ?? $"produto-{id}" : source.Slug),
            Preco = source.Price ?? 0m,
            ImagemUrl = NormalizeAssetPath(primaryImage),
            Categoria = categoria,
            Descricao = source.Description ?? string.Empty,
            Destaque = source.Highlight ?? false,
            Tamanhos = source.Sizes?.Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .ToList()
                ?? new List<string>(),
            ImagensAdicionais = extras.Select(NormalizeAssetPath).ToList()
        };
    }

    private static ProdutoJson MapToJson(ProdutoViewModel source)
    {
        var images = new List<string> { NormalizeAssetPath(source.ImagemUrl) };
        images.AddRange(source.ImagensAdicionais.Select(NormalizeAssetPath));

        return new ProdutoJson
        {
            Id = source.Id.ToString(),
            Name = source.Nome.Trim(),
            Slug = Slugify(string.IsNullOrWhiteSpace(source.Slug) ? source.Nome : source.Slug),
            Sku = $"SKU-{source.Id:000}",
            Price = source.Preco,
            Currency = "USD",
            Description = source.Descricao.Trim(),
            CategoryId = MapCategoryNameToId(source.Categoria),
            Images = images,
            Stock = 100,
            Sizes = source.Tamanhos
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Highlight = source.Destaque
        };
    }

    private static ProdutoViewModel CloneProduto(ProdutoViewModel source)
    {
        return new ProdutoViewModel
        {
            Id = source.Id,
            Nome = source.Nome,
            Slug = source.Slug,
            Preco = source.Preco,
            ImagemUrl = source.ImagemUrl,
            Categoria = source.Categoria,
            Descricao = source.Descricao,
            Destaque = source.Destaque,
            Tamanhos = source.Tamanhos.ToList(),
            ImagensAdicionais = source.ImagensAdicionais.ToList()
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

    private static string MapCategoryIdToName(string? categoryId)
    {
        return categoryId?.Trim().ToLowerInvariant() switch
        {
            "cat-clothing" => "Clothing",
            "cat-accessories" => "Accessories",
            _ => string.IsNullOrWhiteSpace(categoryId) ? "Clothing" : categoryId
        };
    }

    private static string MapCategoryNameToId(string categoryName)
    {
        return categoryName.Trim().ToLowerInvariant() switch
        {
            "clothing" => "cat-clothing",
            "accessories" => "cat-accessories",
            _ => "cat-clothing"
        };
    }

    private static List<ProdutoViewModel> BuildDefaultProducts()
    {
        return new List<ProdutoViewModel>
        {
            new()
            {
                Id = 1,
                Nome = "Shadow Overcoat",
                Slug = "shadow-overcoat",
                Preco = 349m,
                ImagemUrl = "/catalog/shadow-overcoat.svg",
                Categoria = "Outerwear",
                Descricao = "A floor-grazing silhouette cut from heavyweight wool-blend cloth. The Shadow Overcoat drapes the body in architectural darkness.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" }
            },
            new()
            {
                Id = 2,
                Nome = "Phantom Bomber",
                Slug = "phantom-bomber",
                Preco = 289m,
                ImagemUrl = "/catalog/phantom-bomber.svg",
                Categoria = "Outerwear",
                Descricao = "Matte nylon shell with washed silk lining. Minimal hardware and maximum intent.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" }
            },
            new()
            {
                Id = 3,
                Nome = "Noir Trench",
                Slug = "noir-trench",
                Preco = 389m,
                ImagemUrl = "/catalog/noir-trench.svg",
                Categoria = "Outerwear",
                Descricao = "Double-breasted and cut from water-resistant cotton twill for refined drama.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" }
            },
            new()
            {
                Id = 4,
                Nome = "Obsidian Parka",
                Slug = "obsidian-parka",
                Preco = 329m,
                ImagemUrl = "/catalog/obsidian-parka.svg",
                Categoria = "Outerwear",
                Descricao = "Insulated and waxed for winter nights, with deep pockets and shadowed silhouette.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL" }
            },
            new()
            {
                Id = 5,
                Nome = "Silk Noir Shirt",
                Slug = "silk-noir-shirt",
                Preco = 189m,
                ImagemUrl = "/catalog/silk-noir-shirt.svg",
                Categoria = "Shirts",
                Descricao = "Pure silk with hidden placket and clean drape.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" }
            },
            new()
            {
                Id = 6,
                Nome = "Ghost Linen Tee",
                Slug = "ghost-linen-tee",
                Preco = 89m,
                ImagemUrl = "/catalog/ghost-linen-tee.svg",
                Categoria = "Shirts",
                Descricao = "Washed linen essential with relaxed shoulder and breathable structure.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" }
            },
            new()
            {
                Id = 7,
                Nome = "Cartel Oxford",
                Slug = "cartel-oxford",
                Preco = 149m,
                ImagemUrl = "/catalog/cartel-oxford.svg",
                Categoria = "Shirts",
                Descricao = "Structured Japanese cotton shirt balancing ceremony and edge.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL" }
            },
            new()
            {
                Id = 8,
                Nome = "Midnight Henley",
                Slug = "midnight-henley",
                Preco = 109m,
                ImagemUrl = "/catalog/midnight-henley.svg",
                Categoria = "Shirts",
                Descricao = "Heavyweight slub cotton henley for after-hours layering.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" }
            },
            new()
            {
                Id = 9,
                Nome = "Eclipse Trousers",
                Slug = "eclipse-trousers",
                Preco = 179m,
                ImagemUrl = "/catalog/eclipse-trousers.svg",
                Categoria = "Trousers",
                Descricao = "Wide-leg fluid wool trousers inspired by Italian cinema silhouettes.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" }
            },
            new()
            {
                Id = 10,
                Nome = "Sovereign Cargos",
                Slug = "sovereign-cargos",
                Preco = 199m,
                ImagemUrl = "/catalog/sovereign-cargos.svg",
                Categoria = "Trousers",
                Descricao = "Articulated cargo trousers with concealed utility pockets.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" }
            },
            new()
            {
                Id = 11,
                Nome = "Gold Signet Ring",
                Slug = "gold-signet-ring",
                Preco = 129m,
                ImagemUrl = "/catalog/gold-signet-ring.svg",
                Categoria = "Accessories",
                Descricao = "18k gold-plated signet with matte face and engraved crest.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L" }
            },
            new()
            {
                Id = 12,
                Nome = "Onyx Chain",
                Slug = "onyx-chain",
                Preco = 159m,
                ImagemUrl = "/catalog/onyx-chain.svg",
                Categoria = "Accessories",
                Descricao = "Black rhodium-plated silver chain with faceted onyx beads.",
                Destaque = true,
                Tamanhos = new List<string>()
            }
        };
    }

    private sealed class ProdutoJson
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
        public int? Stock { get; set; }
        public List<string>? Sizes { get; set; }
        public string? Slug { get; set; }
        public bool? Highlight { get; set; }
    }
}
