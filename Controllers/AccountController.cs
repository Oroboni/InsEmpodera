using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Models;
using SQLitePCL;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace InsEmpodera.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _context;

    public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(String Email, String Password)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == Email && u.Senha == Password);

        
        if (user != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    public IActionResult Forgot()
    {
        return View();
    }

    public IActionResult Logout()
    {
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
