using System.Text.Json;
using MafiaStore.Data;
using MafiaStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

public class ProdutosController : Controller
{
    private readonly IProductCatalogService _productCatalog;
    private readonly ApplicationDbContext _db;

    public ProdutosController(IProductCatalogService productCatalog, ApplicationDbContext db)
    {
        _productCatalog = productCatalog;
        _db = db;
    }

    public async Task<IActionResult> Index(string? categoria, string? pesquisa, string? ordem, int pagina = 1)
    {
        const int pageSize = 8;
        var filteredQuery = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var categoriaFiltro = categoria.Trim().ToLowerInvariant();
            filteredQuery = filteredQuery.Where(p => p.Category != null && p.Category.Name.ToLower() == categoriaFiltro);
        }

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var search = pesquisa.Trim().ToLowerInvariant();
            filteredQuery = filteredQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(search)));
        }

        var totalItems = await filteredQuery.CountAsync();
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)pageSize);
        pagina = Math.Max(1, pagina);
        pagina = Math.Min(pagina, totalPages);

        List<ProdutoViewModel> produtos;
        var skip = (pagina - 1) * pageSize;

        if (ordem is "preco-asc" or "preco-desc")
        {
            var raw = await filteredQuery
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Slug,
                    p.Price,
                    p.ImageUrl,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    p.Description,
                    p.Highlight,
                    p.SizesJson,
                    p.AdditionalImagesJson
                })
                .ToListAsync();

            var ordered = ordem == "preco-asc"
                ? raw.OrderBy(p => p.Price)
                : raw.OrderByDescending(p => p.Price);

            produtos = ordered
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProdutoViewModel
                {
                    Id = p.Id,
                    Nome = p.Name,
                    Slug = p.Slug,
                    Preco = p.Price,
                    ImagemUrl = p.ImageUrl,
                    Categoria = p.CategoryName,
                    Descricao = p.Description,
                    Destaque = p.Highlight,
                    Tamanhos = ParseListJson(p.SizesJson),
                    ImagensAdicionais = ParseListJson(p.AdditionalImagesJson)
                })
                .ToList();
        }
        else
        {
            produtos = await filteredQuery
                .OrderByDescending(p => p.Id)
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProdutoViewModel
                {
                    Id = p.Id,
                    Nome = p.Name,
                    Slug = p.Slug,
                    Preco = p.Price,
                    ImagemUrl = p.ImageUrl,
                    Categoria = p.Category != null ? p.Category.Name : "Uncategorized",
                    Descricao = p.Description,
                    Destaque = p.Highlight,
                    Tamanhos = ParseListJson(p.SizesJson),
                    ImagensAdicionais = ParseListJson(p.AdditionalImagesJson)
                })
                .ToListAsync();
        }

        ViewBag.CategoriaAtual = categoria;
        ViewBag.PesquisaAtual = pesquisa;
        ViewBag.OrdemAtual = ordem;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.Categorias = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();

        return View(produtos);
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        var produto = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null)
        {
            return NotFound();
        }

        var produtoVm = new ProdutoViewModel
        {
            Id = produto.Id,
            Nome = produto.Name,
            Slug = produto.Slug,
            Preco = produto.Price,
            ImagemUrl = produto.ImageUrl,
            Categoria = produto.Category?.Name ?? "Uncategorized",
            Descricao = produto.Description,
            Destaque = produto.Highlight,
            Tamanhos = ParseListJson(produto.SizesJson),
            ImagensAdicionais = ParseListJson(produto.AdditionalImagesJson)
        };

        var relacionadosRaw = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == produto.CategoryId && p.Id != produto.Id)
            .Take(4)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.ImageUrl,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                p.Description,
                p.Highlight,
                p.SizesJson,
                p.AdditionalImagesJson
            })
            .ToListAsync();

        var relacionados = relacionadosRaw
            .Select(p => new ProdutoViewModel
            {
                Id = p.Id,
                Nome = p.Name,
                Slug = p.Slug,
                Preco = p.Price,
                ImagemUrl = p.ImageUrl,
                Categoria = p.CategoryName,
                Descricao = p.Description,
                Destaque = p.Highlight,
                Tamanhos = ParseListJson(p.SizesJson),
                ImagensAdicionais = ParseListJson(p.AdditionalImagesJson)
            })
            .ToList();

        ViewBag.Relacionados = relacionados;

        return View(produtoVm);
    }

    private static List<string> ParseListJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }
}
