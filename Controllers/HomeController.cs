using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models;
using MafiaStore.Models.ViewModels;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

public class HomeController : Controller
{
    private readonly IProductCatalogService _catalog;

    public HomeController(IProductCatalogService catalog)
    {
        _catalog = catalog;
    }

    public IActionResult Index()
    {
        var products = _catalog.GetAll().ToList();

        var formalProducts = products
            .Where(p =>
                string.Equals(p.Categoria, "Clothing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Categoria, "Outerwear", StringComparison.OrdinalIgnoreCase) ||
                ContainsAny(p.Nome, "suit", "blazer", "oxford", "moccasin", "fato", "terno", "formal"))
            .ToList();

        var featured = new List<ProdutoViewModel>();

        var suitCandidates = formalProducts
            .Where(p => ContainsAny(p.Nome, "suit", "blazer", "fato", "terno"))
            .OrderByDescending(p => p.Destaque)
            .ThenByDescending(p => p.Preco)
            .ThenByDescending(p => p.Id)
            .ToList();

        featured.AddRange(suitCandidates.Take(3));

        var formalExtra = formalProducts
            .Where(p => featured.All(f => f.Id != p.Id))
            .Where(p => ContainsAny(p.Nome, "oxford", "moccasin", "loafer", "dress", "formal", "social", "polo"))
            .OrderByDescending(p => p.Destaque)
            .ThenByDescending(p => p.Preco)
            .ThenByDescending(p => p.Id)
            .FirstOrDefault();

        if (formalExtra is not null)
        {
            featured.Add(formalExtra);
        }

        if (featured.Count < 4)
        {
            featured.AddRange(
                products
                    .Where(p => featured.All(f => f.Id != p.Id))
                    .OrderByDescending(p => p.Destaque)
                    .ThenByDescending(p => p.Preco)
                    .ThenByDescending(p => p.Id)
                    .Take(4 - featured.Count));
        }

        return View(featured.Take(4).ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static bool ContainsAny(string? text, params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
