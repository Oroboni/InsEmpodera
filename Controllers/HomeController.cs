using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace InsEmpodera.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View();
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
