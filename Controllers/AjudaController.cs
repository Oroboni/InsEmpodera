using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers;

public sealed class AjudaController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") is null)
            return RedirectToAction("Index", "Account");

        return View();
    }
}
