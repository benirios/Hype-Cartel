using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class OrdersAdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "orders" });
    }

    [HttpGet]
    public IActionResult Detalhes(int id)
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "orders", orderId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(int orderId, string newStatus)
    {
        TempData["AdminError"] = "Legacy order status endpoint is disabled. Use the Dashboard orders tab.";
        return RedirectToAction("Dashboard", "Admin", new { tab = "orders", orderId });
    }

    [HttpGet]
    public IActionResult History()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "orders" });
    }
}
