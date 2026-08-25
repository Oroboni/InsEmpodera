using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string Email, string Password)
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ViewData["LoginError"] = "Informe o e-mail e a senha.";
            return View();
        }

        var normalizedEmail = Email.Trim().ToUpperInvariant();
        var user = await _context.Usuarios.FirstOrDefaultAsync(u =>
            u.Ativo == "S" && u.Email.ToUpper() == normalizedEmail);
        if (user == null)
        {
            ViewData["LoginError"] = "E-mail ou senha inválidos.";
            return View();
        }

        var hasher = new PasswordHasher<Usuario>();
        var result = hasher.VerifyHashedPassword(user, user.Senha, Password);
        if (result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
        {
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Senha = hasher.HashPassword(user, Password);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Nome", user.Nome);
            HttpContext.Session.SetString("ID", user.IdUsuario.ToString());
            if (!UserCultureService.HasSavedMode(Request))
                UserCultureService.FollowBrowser(Response);
            return RedirectToAction("Index", "Home");
        }

        ViewData["LoginError"] = "E-mail ou senha inválidos.";
        return View();
    }

    public IActionResult Forgot()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
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
