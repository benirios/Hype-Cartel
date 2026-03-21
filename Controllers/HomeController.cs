using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models;
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
        var highlights = _catalog.GetAll().Where(p => p.Destaque).ToList();
        return View(highlights);
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
}
