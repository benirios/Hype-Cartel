using Microsoft.AspNetCore.Mvc;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

public class CarrinhoController : Controller
{
    private readonly ICartStore _cartStore;

    public CarrinhoController(ICartStore cartStore)
    {
        _cartStore = cartStore;
    }

    public IActionResult Index()
    {
        var items = _cartStore.GetItems();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Adicionar(int produtoId, string? tamanho)
    {
        if (!_cartStore.AddItem(produtoId, tamanho))
        {
            return BadRequest();
        }

        var referer = Request.Headers.Referer.ToString();
        return string.IsNullOrWhiteSpace(referer)
            ? RedirectToAction("Index")
            : Redirect(referer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AtualizarQuantidade(int produtoId, string? tamanho, int quantidade)
    {
        if (!_cartStore.UpdateQuantity(produtoId, tamanho, quantidade))
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int produtoId, string? tamanho)
    {
        if (!_cartStore.RemoveItem(produtoId, tamanho))
        {
            return NotFound();
        }

        return Ok();
    }
}
