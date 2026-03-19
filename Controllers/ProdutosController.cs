using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

public class ProdutosController : Controller
{
    private readonly IProductCatalogService _productCatalog;

    public ProdutosController(IProductCatalogService productCatalog)
    {
        _productCatalog = productCatalog;
    }

    public IActionResult Index(string? categoria, string? pesquisa, string? ordem)
    {
        var produtos = _productCatalog.GetAll().ToList();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            produtos = produtos
                .Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var search = pesquisa.ToLower();
            produtos = produtos
                .Where(p => p.Nome.ToLower().Contains(search) ||
                            p.Descricao.ToLower().Contains(search) ||
                            p.Categoria.ToLower().Contains(search))
                .ToList();
        }

        produtos = ordem switch
        {
            "preco-asc"  => produtos.OrderBy(p => p.Preco).ToList(),
            "preco-desc" => produtos.OrderByDescending(p => p.Preco).ToList(),
            _            => produtos
        };

        ViewBag.CategoriaAtual = categoria;
        ViewBag.PesquisaAtual = pesquisa;
        ViewBag.OrdemAtual = ordem;

        return View(produtos);
    }

    public IActionResult Detalhes(int id)
    {
        var produtos = _productCatalog.GetAll().ToList();
        var produto = _productCatalog.GetById(id);

        if (produto == null)
        {
            return NotFound();
        }

        var relacionados = produtos
            .Where(p => p.Categoria == produto.Categoria && p.Id != produto.Id)
            .Take(4)
            .ToList();

        ViewBag.Relacionados = relacionados;

        return View(produto);
    }
}
