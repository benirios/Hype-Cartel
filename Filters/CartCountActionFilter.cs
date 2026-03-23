using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MafiaStore.Services;

namespace MafiaStore.Filters;

public sealed class CartCountActionFilter : IActionFilter
{
    private readonly ICartStore _cartStore;
    private readonly ICartOwnerResolver _cartOwnerResolver;

    public CartCountActionFilter(ICartStore cartStore, ICartOwnerResolver cartOwnerResolver)
    {
        _cartStore = cartStore;
        _cartOwnerResolver = cartOwnerResolver;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            var ownerKey = _cartOwnerResolver.ResolveCurrentOwnerKey();
            controller.ViewBag.CartCount = _cartStore.GetCartCount(ownerKey);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
