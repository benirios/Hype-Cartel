using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MafiaStore.Services;

namespace MafiaStore.Filters;

public sealed class CartCountActionFilter : IActionFilter
{
    private readonly ICartStore _cartStore;

    public CartCountActionFilter(ICartStore cartStore)
    {
        _cartStore = cartStore;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.CartCount = _cartStore.GetCartCount();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
