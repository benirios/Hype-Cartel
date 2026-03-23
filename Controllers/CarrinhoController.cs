using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MafiaStore.Services;

namespace MafiaStore.Controllers;

public class CarrinhoController : Controller
{
    private readonly ICartStore _cartStore;
    private readonly ICartOwnerResolver _cartOwnerResolver;
    private readonly IOrderService _orderService;

    public CarrinhoController(
        ICartStore cartStore,
        ICartOwnerResolver cartOwnerResolver,
        IOrderService orderService)
    {
        _cartStore = cartStore;
        _cartOwnerResolver = cartOwnerResolver;
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
        var items = _cartStore.GetItems(ownerKey);
        if (TempData["CheckoutError"] is string checkoutError)
        {
            ViewBag.CheckoutError = checkoutError;
        }

        if (TempData["CheckoutSuccess"] is string checkoutSuccess)
        {
            ViewBag.CheckoutSuccess = checkoutSuccess;
        }

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Adicionar(int produtoId, string? tamanho)
    {
        var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
        if (!_cartStore.AddItem(ownerKey, produtoId, tamanho))
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
        var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
        if (!_cartStore.UpdateQuantity(ownerKey, produtoId, tamanho, quantidade))
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remover(int produtoId, string? tamanho)
    {
        var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
        if (!_cartStore.RemoveItem(ownerKey, produtoId, tamanho))
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("/Carrinho/Checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = User.Identity?.Name;
        }
        userId ??= ownerKey;

        var result = await _orderService.CreateOrderAsync(userId, ownerKey);
        if (!result.Success)
        {
            TempData["CheckoutError"] = result.ErrorMessage ?? "Checkout failed.";
            return RedirectToAction(nameof(Index));
        }

        TempData["CheckoutSuccess"] = $"Order #{result.OrderId} created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
