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
        ViewBag.Comunidade = _context.Comunidades.ToList();
        ViewBag.Atores = _context.Atores.ToList();
        return View();
    }

    public IActionResult HomePage()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Index.cshtml");
    }

    public IActionResult Atores()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Actor/Index.cshtml");
    }

    public IActionResult FichaPrimeiroContato()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/FichaPrimeiroContato/Index.cshtml");
    }

    public IActionResult DiariosDeCampo()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/DiariosDeCampo.cshtml");
    }

    public IActionResult DiarioProcessoPessoal()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/DiarioProcessoPessoal/Index.cshtml");
    }

    public IActionResult Relatorios()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Report/Index.cshtml");
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Dashboard/Index.cshtml");
    }

    public IActionResult Atividades()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Activity/index.cshtml");
    }

    public IActionResult Usuarios()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Users/index.cshtml");
    }

    public IActionResult PerfisDeAcesso()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/AccessProfile/Index.cshtml");
    }

    public IActionResult Ajuda()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Ajuda.cshtml");
    }

    public IActionResult Configuracoes()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        return View("~/Views/Home/Sidebar/Configuracoes.cshtml");
    }

    public IActionResult Logout()
    {
        // espaçozinho para talvez futura lógica de logout
        return RedirectToAction("Home");
    }
}
