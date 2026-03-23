using System.Text.Json;
using MafiaStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MafiaStore.Data;

public static class LegacyJsonDataMigrator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task MigrateAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await MigrateProductsAsync(db, environment);
        await MigrateUsersAsync(userManager, roleManager, environment);
    }

    private static async Task MigrateProductsAsync(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        var productsPath = Path.Combine(environment.ContentRootPath, "Catalog_Assets", "products.json");
        if (!File.Exists(productsPath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(productsPath);
        var legacyProducts = JsonSerializer.Deserialize<List<LegacyProduct>>(json, JsonOptions) ?? new();
        if (legacyProducts.Count == 0)
        {
            return;
        }

        var categorySlugs = legacyProducts
            .Select(x => NormalizeCategorySlug(x.CategoryId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var slug in categorySlugs)
        {
            if (!await db.Categories.AnyAsync(c => c.Slug == slug))
            {
                db.Categories.Add(new Category
                {
                    Name = CategoryDisplayNameFromSlug(slug),
                    Slug = slug
                });
            }
        }

        await db.SaveChangesAsync();
        var categoriesBySlug = await db.Categories.ToDictionaryAsync(c => c.Slug, StringComparer.OrdinalIgnoreCase);

        foreach (var source in legacyProducts)
        {
            if (!int.TryParse(source.Id, out var productId) || productId <= 0)
            {
                continue;
            }

            if (await db.Products.AnyAsync(p => p.Id == productId))
            {
                continue;
            }

            var categorySlug = NormalizeCategorySlug(source.CategoryId);
            if (!categoriesBySlug.TryGetValue(categorySlug, out var category))
            {
                continue;
            }

            var images = source.Images?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new();
            var imageUrl = images.FirstOrDefault() ?? "/catalog/placeholder.svg";
            var extraImages = images.Skip(1).ToList();
            var sizes = source.Sizes?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new();

            db.Products.Add(new Product
            {
                Id = productId,
                Name = string.IsNullOrWhiteSpace(source.Name) ? $"Product {productId}" : source.Name.Trim(),
                Slug = string.IsNullOrWhiteSpace(source.Slug)
                    ? Slugify(source.Name ?? $"product-{productId}")
                    : Slugify(source.Slug),
                Sku = string.IsNullOrWhiteSpace(source.Sku) ? $"SKU-{productId:000}" : source.Sku.Trim(),
                Price = source.Price ?? 0m,
                Description = source.Description?.Trim() ?? string.Empty,
                ImageUrl = imageUrl,
                AdditionalImagesJson = JsonSerializer.Serialize(extraImages),
                SizesJson = JsonSerializer.Serialize(sizes),
                Stock = source.Stock ?? 0,
                Highlight = source.Highlight ?? false,
                CategoryId = category.Id
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task MigrateUsersAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment)
    {
        var usersPath = Path.Combine(environment.ContentRootPath, "context", "users.json");
        if (!File.Exists(usersPath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(usersPath);
        var legacyUsers = JsonSerializer.Deserialize<List<LegacyUser>>(json, JsonOptions) ?? new();
        foreach (var legacy in legacyUsers)
        {
            if (string.IsNullOrWhiteSpace(legacy.Username))
            {
                continue;
            }

            var role = string.IsNullOrWhiteSpace(legacy.Role) ? "Customer" : legacy.Role.Trim();
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createRole = await roleManager.CreateAsync(new IdentityRole(role));
                if (!createRole.Succeeded)
                {
                    var errors = string.Join("; ", createRole.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed creating role '{role}': {errors}");
                }
            }

            var user = await userManager.FindByNameAsync(legacy.Username.Trim());
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = legacy.Username.Trim(),
                    Email = string.IsNullOrWhiteSpace(legacy.Email) ? $"{legacy.Username.Trim()}@local" : legacy.Email.Trim(),
                    EmailConfirmed = true
                };

                var createUser = await userManager.CreateAsync(user, "Temp@123");
                if (!createUser.Succeeded)
                {
                    var errors = string.Join("; ", createUser.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed creating user '{legacy.Username}': {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed assigning role '{role}' to '{legacy.Username}': {errors}");
                }
            }
        }
    }

    private static string NormalizeCategorySlug(string? categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return "cat-uncategorized";
        }

        var trimmed = categoryId.Trim().ToLowerInvariant();
        return trimmed.StartsWith("cat-", StringComparison.Ordinal) ? trimmed : $"cat-{trimmed}";
    }

    private static string CategoryDisplayNameFromSlug(string slug)
    {
        return slug switch
        {
            "cat-accessories" => "Accessories",
            "cat-clothing" => "Clothing",
            "cat-outerwear" => "Outerwear",
            "cat-shirts" => "Shirts",
            "cat-trousers" => "Trousers",
            _ => HumanizeSlug(slug.Replace("cat-", string.Empty, StringComparison.Ordinal))
        };
    }

    private static string HumanizeSlug(string slug)
    {
        var chunks = slug.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (chunks.Length == 0)
        {
            return "Uncategorized";
        }

        return string.Join(" ", chunks.Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
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

    private sealed class LegacyProduct
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public List<string>? Images { get; set; }
        public int? Stock { get; set; }
        public List<string>? Sizes { get; set; }
        public string? Slug { get; set; }
        public bool? Highlight { get; set; }
    }

    private sealed class LegacyUser
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }
}
