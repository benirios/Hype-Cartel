using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IProductCatalogService _productCatalog;

    public AdminController(IProductCatalogService productCatalog)
    {
        _productCatalog = productCatalog;
    }

    [HttpGet]
    public IActionResult Produtos()
    {
        var produtos = _productCatalog
            .GetAll()
            .OrderBy(p => p.Id)
            .ToList();

        return View(produtos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(
        string nome,
        decimal preco,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false)
    {
        var produto = new ProdutoViewModel
        {
            Id = _productCatalog.GetNextId(),
            Nome = nome,
            Slug = string.Empty,
            Preco = preco,
            Categoria = categoria,
            ImagemUrl = imagemUrl,
            Descricao = descricao,
            Destaque = destaque,
            Tamanhos = ParseSizes(tamanhos),
            ImagensAdicionais = new List<string>()
        };

        if (!_productCatalog.Create(produto, out var error))
        {
            TempData["AdminError"] = error;
            return RedirectToAction(nameof(Produtos));
        }

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(
        int id,
        string nome,
        decimal preco,
        string categoria,
        string imagemUrl,
        string descricao,
        string? tamanhos,
        bool destaque = false)
    {
        var produtoExistente = _productCatalog.GetById(id);
        if (produtoExistente is null)
        {
            TempData["AdminError"] = "Product not found.";
            return RedirectToAction(nameof(Produtos));
        }

        produtoExistente.Nome = nome;
        produtoExistente.Preco = preco;
        produtoExistente.Categoria = categoria;
        produtoExistente.ImagemUrl = imagemUrl;
        produtoExistente.Descricao = descricao;
        produtoExistente.Destaque = destaque;
        produtoExistente.Tamanhos = ParseSizes(tamanhos);

        if (!_productCatalog.Update(produtoExistente, out var error))
        {
            TempData["AdminError"] = error;
            return RedirectToAction(nameof(Produtos));
        }

        return RedirectToAction(nameof(Produtos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int id)
    {
        _productCatalog.Delete(id);
        return RedirectToAction(nameof(Produtos));
    }

    private static List<string> ParseSizes(string? sizes)
    {
        if (string.IsNullOrWhiteSpace(sizes))
        {
            return new List<string>();
        }

        return sizes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(size => !string.IsNullOrWhiteSpace(size))
            .Select(size => size.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
