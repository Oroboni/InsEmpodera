using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Empodera.Services;

namespace Empodera.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _context;

    public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string Email, string Password)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == Email && u.Ativo != "N");

        if (user == null)
        {
            return View();
        }

        var hasher = new PasswordHasher<Usuario>();
        var result = hasher.VerifyHashedPassword(user, user.Senha, Password);

        if (result == PasswordVerificationResult.Success)
        {
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Nome", user.Nome);
            HttpContext.Session.SetString("ID", user.IdUsuario.ToString());
            UserCultureService.ApplyPreference(Response, user.IdiomaPreferido);
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
        HttpContext.Session.Clear();
        UserCultureService.ClearPreference(Response);
        return RedirectToAction("Index", "Account");
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
