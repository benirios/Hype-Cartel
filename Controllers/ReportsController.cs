using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MafiaStore.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "reports" });
    }

    [HttpGet]
    public IActionResult TopProducts()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "reports" });
    }

    [HttpGet]
    public IActionResult MonthlyRevenue()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "reports" });
    }

    [HttpGet]
    public IActionResult OrderStateDistribution()
    {
        return RedirectToAction("Dashboard", "Admin", new { tab = "reports" });
    }
}
