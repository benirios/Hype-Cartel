using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models.ViewModels;

namespace MafiaStore.Controllers;

public class CarrinhoController : Controller
{
    private static List<CarrinhoItemViewModel> GetCartItems()
    {
        return new List<CarrinhoItemViewModel>
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
    }

    public IActionResult Index()
    {
        var items = GetCartItems();
        return View(items);
    }
}
