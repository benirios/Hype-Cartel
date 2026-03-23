using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MafiaStore.Services;

public interface ICartOwnerResolver
{
    string ResolveCurrentOwnerKey();
}

public sealed class CartOwnerResolver : ICartOwnerResolver
{
    private const string CartCookieName = "MafiaStore.CartOwner";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartOwnerResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string ResolveCurrentOwnerKey()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context.");

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }

            var username = httpContext.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                return $"user-name:{username}";
            }
        }

        if (httpContext.Request.Cookies.TryGetValue(CartCookieName, out var existing) &&
            !string.IsNullOrWhiteSpace(existing))
        {
            return $"anon:{existing}";
        }

        var generated = Guid.NewGuid().ToString("N");
        httpContext.Response.Cookies.Append(
            CartCookieName,
            generated,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = httpContext.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        return $"anon:{generated}";
    }
}
