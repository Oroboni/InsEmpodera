using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.AspNetCore.Authorization;

namespace Empodera.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        ViewBag.Comunidade = _context.Comunidades.Where(a => a.Ativo != "N").ToList();
        ViewBag.Atores = _context.Atores.Where(a => a.Ativo != "N").ToList();
        ViewBag.Usuarios = _context.Usuarios.ToList();
        return View();
    }
    public IActionResult Logout()
    {
        // espaçozinho para talvez futura lógica de logout
        return RedirectToAction("Home");
    }
}
